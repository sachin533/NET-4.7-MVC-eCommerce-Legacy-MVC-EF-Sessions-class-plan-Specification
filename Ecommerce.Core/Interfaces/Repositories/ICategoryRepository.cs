using System.Collections.Generic;
using Ecommerce.Core.Entities;

namespace Ecommerce.Core.Interfaces.Repositories
{
    public interface ICategoryRepository
    {
        /// <summary>Active categories with children eagerly loaded, ordered for the sidebar tree.</summary>
        IList<Category> GetActiveWithChildren();

        Category GetById(int id);

        /// <summary>Direct active-product count per category id (used to label the sidebar tree).</summary>
        IDictionary<int, int> GetActiveProductCounts();
    }
}
