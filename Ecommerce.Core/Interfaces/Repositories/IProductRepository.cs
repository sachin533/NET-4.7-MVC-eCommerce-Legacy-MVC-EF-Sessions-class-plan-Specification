using System.Collections.Generic;
using Ecommerce.Core.Common;
using Ecommerce.Core.Entities;

namespace Ecommerce.Core.Interfaces.Repositories
{
    /// <summary>
    /// Catalog queries. Repositories are the ONLY type in the solution that talks to the database.
    /// </summary>
    public interface IProductRepository
    {
        /// <summary>Paged catalog search: optional category, free-text query, price range and sort.</summary>
        /// <param name="sort">"newest" (default), "name", "price-asc", "price-desc".</param>
        PagedResult<Product> Search(int? categoryId, string query, decimal? minPrice, decimal? maxPrice, string sort, int page, int pageSize);

        /// <summary>Product with Category, Images and Variants eagerly loaded (Include).</summary>
        Product GetById(int id);

        /// <summary>Home page featured products (IsFeatured = 1), newest first.</summary>
        IList<Product> GetFeatured(int count);
    }
}
