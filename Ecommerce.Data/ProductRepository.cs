using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Ecommerce.Core.Common;
using Ecommerce.Core.Entities;
using Ecommerce.Core.Interfaces.Repositories;

namespace Ecommerce.Data
{
    public class ProductRepository : IProductRepository
    {
        private readonly EcommerceDbContext _context;

        public ProductRepository(EcommerceDbContext context)
        {
            _context = context;
        }

        public PagedResult<Product> Search(int? categoryId, string query, decimal? minPrice, decimal? maxPrice, string sort, int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 12;

            IQueryable<Product> q = _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive);

            if (categoryId.HasValue)
                q = q.Where(p => p.CategoryId == categoryId.Value);

            if (!string.IsNullOrWhiteSpace(query))
            {
                string term = query.Trim();
                q = q.Where(p => p.Name.Contains(term) || p.Sku.Contains(term)
                    || p.ShortDescription.Contains(term) || p.Description.Contains(term));
            }

            if (minPrice.HasValue)
                q = q.Where(p => p.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                q = q.Where(p => p.Price <= maxPrice.Value);

            int total = q.Count();

            IOrderedQueryable<Product> ordered;
            switch (sort)
            {
                case "name":
                    ordered = q.OrderBy(p => p.Name);
                    break;
                case "price-asc":
                    ordered = q.OrderBy(p => p.Price).ThenBy(p => p.Name);
                    break;
                case "price-desc":
                    ordered = q.OrderByDescending(p => p.Price).ThenBy(p => p.Name);
                    break;
                default: // "newest"
                    ordered = q.OrderByDescending(p => p.CreatedUtc).ThenBy(p => p.Id);
                    break;
            }

            List<Product> items = ordered
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PagedResult<Product>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalItems = total
            };
        }

        public Product GetById(int id)
        {
            return _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Include(p => p.Variants)
                .FirstOrDefault(p => p.Id == id && p.IsActive);
        }

        public IList<Product> GetFeatured(int count)
        {
            if (count < 1) count = 4;
            return _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive && p.IsFeatured)
                .OrderByDescending(p => p.CreatedUtc)
                .Take(count)
                .ToList();
        }
    }
}
