using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Ecommerce.Web.App_Start;

namespace Ecommerce.Web
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            UnityConfig.RegisterComponents();

            // Security: every POST action is protected by [ValidateAntiForgeryToken].
            // NOTE on the spec's "token via AJAX header" sketch: the pinned stack
            // (MVC 5.2.7 + WebPages 3.2.7) has no anti-forgery header support — its reader
            // only accepts the __RequestVerificationToken FORM FIELD (verified against the
            // shipped assemblies). Scripts/site.js therefore injects the token into every
            // AJAX POST's form data (plus a RequestVerificationToken header for
            // forward-compat), which validates here without any extra configuration.
        }

        protected void Application_BeginRequest()
        {
            UnityConfig.BeginRequest();
        }

        protected void Application_EndRequest()
        {
            // Disposes the per-request Unity child container (see UnityConfig).
            UnityConfig.EndRequest();
        }
    }
}
