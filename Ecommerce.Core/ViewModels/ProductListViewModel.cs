using System.Collections.Generic;

namespace Ecommerce.Core.ViewModels
{
    /// <summary>
    /// Catalog list page state: category filter, free-text query, paging + the page of cards.
    /// Bound by Views/Product/Index.cshtml and re-bound by the AJAX filter (_ProductList partial).
    /// </summary>
    public class ProductListViewModel
    {
        public ProductListViewModel()
        {
            Products = new List<ProductCardViewModel>();
            Categories = new List<CategoryTreeNodeViewModel>();
            Sort = "newest";
        }

        public IList<ProductCardViewModel> Products { get; set; }
        public IList<CategoryTreeNodeViewModel> Categories { get; set; }

        public int? SelectedCategoryId { get; set; }
        public string SelectedCategoryName { get; set; }
        public string Query { get; set; }

        /// <summary>Sort: "newest" (default), "name", "price-asc", "price-desc".</summary>
        public string Sort { get; set; }

        /// <summary>Optional price range filter (inclusive, USD).</summary>
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }

        public bool HasPreviousPage { get { return Page > 1; } }
        public bool HasNextPage { get { return Page < TotalPages; } }
    }
}
