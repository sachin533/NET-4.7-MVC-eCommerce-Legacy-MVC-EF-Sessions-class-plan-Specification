using System.Web.Mvc;
using System.Web.Routing;

namespace Ecommerce.Web.App_Start
{
    public static class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // Pretty product URLs: /product/12  (spec sketch: Url.Action("Detail", "Product", new { id }))
            routes.MapRoute(
                name: "ProductDetail",
                url: "product/{id}",
                defaults: new { controller = "Product", action = "Detail" },
                constraints: new { id = @"\d+" });

            // Catalog: /products
            routes.MapRoute(
                name: "Catalog",
                url: "products",
                defaults: new { controller = "Product", action = "Index" });

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional });
        }
    }
}
