using System;
using System.Collections.Generic;

namespace Ecommerce.Core.Common
{
    /// <summary>
    /// Standard paging envelope returned by repository/service list queries.
    /// </summary>
    public class PagedResult<T>
    {
        public PagedResult()
        {
            Items = new List<T>();
        }

        public IList<T> Items { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }

        public int TotalPages
        {
            get { return PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalItems / (double)PageSize); }
        }

        public bool HasPreviousPage
        {
            get { return Page > 1; }
        }

        public bool HasNextPage
        {
            get { return Page < TotalPages; }
        }
    }
}
