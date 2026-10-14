using System.Linq;
using System.Web.Mvc;
using Ecommerce.Core.Interfaces.Services;
using Ecommerce.Core.ViewModels;

namespace Ecommerce.Web.Controllers
{
    /// <summary>
    /// Catalog: list with category filter + search (server-rendered, re-filtered via AJAX
    /// against Product/Filter), and the product detail page with the Fancybox gallery.
    /// </summary>
    public class ProductController : Controller
    {
        public const int PageSize = 9;

        private readonly ICatalogService _catalog;

        public ProductController(ICatalogService catalog)
        {
            _catalog = catalog;
        }

        /// <summary>Catalog page — also reachable via the /products route.</summary>
        public ActionResult Index(int? category, string q, string sort, decimal? priceMin, decimal? priceMax, int page = 1)
        {
            ProductListViewModel model = _catalog.GetProductList(category, q, sort, priceMin, priceMax, page, PageSize);
            return View(model);
        }

        /// <summary>
        /// AJAX filter: returns only the grid + paging markup (see Scripts/site.js).
        /// </summary>
        public ActionResult Filter(int? category, string q, string sort, decimal? priceMin, decimal? priceMax, int page = 1)
        {
            ProductListViewModel model = _catalog.GetProductList(category, q, sort, priceMin, priceMax, page, PageSize);
            return PartialView("_ProductList", model);
        }

        /// <summary>Product detail — also reachable via the /product/{id} route.</summary>
        public ActionResult Detail(int id)
        {
            ProductDetailViewModel model = _catalog.GetProductDetail(id);
            if (model == null)
            {
                return HttpNotFound();
            }
            return View(model);
        }

        /// <summary>
        /// Navbar "Categories" dropdown — rendered on every page through the shared layout
        /// (&#64;Html.Action child action; never routable on its own).
        /// </summary>
        [ChildActionOnly]
        public ActionResult NavCategories()
        {
            return PartialView("_NavCategories", _catalog.GetCategoryTree());
        }

        /// <summary>
        /// "You may also like" strip under a product detail page (featured items, minus the
        /// product currently being viewed).
        /// </summary>
        [ChildActionOnly]
        public ActionResult RelatedProducts(int excludeId)
        {
            var items = _catalog.GetFeaturedProducts(8)
                .Where(p => p.Id != excludeId)
                .Take(4)
                .ToList();
            return PartialView("_RelatedProducts", items);
        }
    }
}
