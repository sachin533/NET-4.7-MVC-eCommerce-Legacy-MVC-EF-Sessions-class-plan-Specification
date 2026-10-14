using System;
using System.Collections.Generic;
using System.Linq;
using Ecommerce.Core.Entities;
using Ecommerce.Core.Interfaces.Repositories;
using Ecommerce.Core.Interfaces.Services;
using Ecommerce.Core.ViewModels;
using Ecommerce.Services.Mapping;

namespace Ecommerce.Services
{
    /// <summary>
    /// Catalog read side: list/filter/paging, detail page, featured products, category tree.
    /// Controller -> Service -> Repository -> EcommerceDbContext; the controller never queries.
    /// </summary>
    public class CatalogService : ICatalogService
    {
        private readonly IProductRepository _products;
        private readonly ICategoryRepository _categories;

        public CatalogService(IProductRepository products, ICategoryRepository categories)
        {
            _products = products;
            _categories = categories;
        }

        public ProductListViewModel GetProductList(int? categoryId, string query, string sort, decimal? minPrice, decimal? maxPrice, int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 12;
            if (pageSize > 96) pageSize = 96;

            // Whitelist the sort — never pass user input through unchecked.
            if (sort != "name" && sort != "price-asc" && sort != "price-desc")
            {
                sort = "newest";
            }

            // Normalise the price range: negatives are meaningless, swapped bounds are fixed.
            if (minPrice.HasValue && minPrice.Value < 0) minPrice = 0;
            if (maxPrice.HasValue && maxPrice.Value < 0) maxPrice = null;
            if (minPrice.HasValue && maxPrice.HasValue && minPrice.Value > maxPrice.Value)
            {
                decimal swap = minPrice.Value;
                minPrice = maxPrice.Value;
                maxPrice = swap;
            }

            var tree = GetCategoryTree();
            var result = _products.Search(categoryId, query, minPrice, maxPrice, sort, page, pageSize);

            var vm = new ProductListViewModel
            {
                Products = result.Items.Select(EntityMapper.ToCard).ToList(),
                Categories = tree,
                SelectedCategoryId = categoryId,
                SelectedCategoryName = FindCategoryName(tree, categoryId),
                Query = query,
                Sort = sort,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                Page = result.Page,
                PageSize = result.PageSize,
                TotalItems = result.TotalItems,
                TotalPages = result.TotalPages
            };

            return vm;
        }

        public ProductDetailViewModel GetProductDetail(int productId)
        {
            var product = _products.GetById(productId);
            if (product == null || !product.IsActive) return null;

            return EntityMapper.ToDetail(product);
        }

        public IList<ProductCardViewModel> GetFeaturedProducts(int count)
        {
            return _products.GetFeatured(count).Select(EntityMapper.ToCard).ToList();
        }

        public IList<CategoryTreeNodeViewModel> GetCategoryTree()
        {
            var categories = _categories.GetActiveWithChildren();
            var counts = _categories.GetActiveProductCounts();

            var roots = categories
                .Where(c => !c.ParentCategoryId.HasValue)
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name)
                .ToList();

            return roots.Select(c => BuildNode(c, counts, 0)).ToList();
        }

        private static CategoryTreeNodeViewModel BuildNode(Category category, IDictionary<int, int> counts, int depth)
        {
            // Guard against accidental cycles in the self-referencing FK.
            if (depth > 8) return null;

            var node = new CategoryTreeNodeViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Slug = category.Slug,
                IsActive = category.IsActive
            };

            int direct;
            counts.TryGetValue(category.Id, out direct);
            node.ProductCount = direct;

            if (category.ChildCategories != null)
            {
                foreach (var child in category.ChildCategories
                    .Where(ch => ch.IsActive)
                    .OrderBy(ch => ch.DisplayOrder)
                    .ThenBy(ch => ch.Name))
                {
                    var childNode = BuildNode(child, counts, depth + 1);
                    if (childNode == null) continue;
                    node.Children.Add(childNode);
                    node.ProductCount += childNode.ProductCount;
                }
            }

            return node;
        }

        private static string FindCategoryName(IList<CategoryTreeNodeViewModel> tree, int? categoryId)
        {
            if (!categoryId.HasValue) return null;

            foreach (var node in tree)
            {
                var name = FindCategoryName(node, categoryId.Value);
                if (name != null) return name;
            }

            return null;
        }

        private static string FindCategoryName(CategoryTreeNodeViewModel node, int categoryId)
        {
            if (node.Id == categoryId) return node.Name;

            foreach (var child in node.Children)
            {
                var name = FindCategoryName(child, categoryId);
                if (name != null) return name;
            }

            return null;
        }
    }
}
