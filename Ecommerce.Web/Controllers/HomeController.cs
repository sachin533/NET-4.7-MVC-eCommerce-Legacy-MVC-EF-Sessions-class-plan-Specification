using System.Web.Mvc;
using Ecommerce.Core.Interfaces.Services;
using Ecommerce.Core.ViewModels;

namespace Ecommerce.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ICatalogService _catalog;

        public HomeController(ICatalogService catalog)
        {
            _catalog = catalog;
        }

        /// <summary>Home page — hero categories + featured products.</summary>
        public ActionResult Index()
        {
            var model = new HomeIndexViewModel
            {
                Categories = _catalog.GetCategoryTree(),
                Featured = _catalog.GetFeaturedProducts(8)
            };
            return View(model);
        }
    }
}
