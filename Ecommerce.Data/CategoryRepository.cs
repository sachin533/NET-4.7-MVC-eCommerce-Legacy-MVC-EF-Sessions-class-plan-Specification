using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Ecommerce.Core.Entities;
using Ecommerce.Core.Interfaces.Repositories;

namespace Ecommerce.Data
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly EcommerceDbContext _context;

        public CategoryRepository(EcommerceDbContext context)
        {
            _context = context;
        }

        public IList<Category> GetActiveWithChildren()
        {
            return _context.Categories
                .Include(c => c.ChildCategories)
                .Where(c => c.IsActive && c.ParentCategoryId == null)
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name)
                .ToList();
        }

        public Category GetById(int id)
        {
            return _context.Categories.FirstOrDefault(c => c.Id == id && c.IsActive);
        }

        public IDictionary<int, int> GetActiveProductCounts()
        {
            return _context.Products
                .Where(p => p.IsActive)
                .GroupBy(p => p.CategoryId)
                .Select(g => new { CategoryId = g.Key, Count = g.Count() })
                .ToList()
                .ToDictionary(x => x.CategoryId, x => x.Count);
        }
    }
}
