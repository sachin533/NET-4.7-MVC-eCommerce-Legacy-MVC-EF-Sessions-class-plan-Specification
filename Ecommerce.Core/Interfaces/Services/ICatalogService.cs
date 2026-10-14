using System.Collections.Generic;
using Ecommerce.Core.ViewModels;

namespace Ecommerce.Core.Interfaces.Services
{
    /// <summary>
    /// Catalog read side: list/filter/paging, detail page and the sidebar category tree.
    /// Maps entities → ViewModels; no controller or SQL concerns here.
    /// </summary>
    public interface ICatalogService
    {
        /// <param name="sort">"newest" (default), "name", "price-asc", "price-desc" — unknown values fall back to "newest".</param>
        ProductListViewModel GetProductList(int? categoryId, string query, string sort, decimal? minPrice, decimal? maxPrice, int page, int pageSize);

        ProductDetailViewModel GetProductDetail(int productId);

        IList<ProductCardViewModel> GetFeaturedProducts(int count);

        IList<CategoryTreeNodeViewModel> GetCategoryTree();
    }
}
