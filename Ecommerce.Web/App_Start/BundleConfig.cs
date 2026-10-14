using System.Web.Optimization;

namespace Ecommerce.Web.App_Start
{
    public static class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            // Core libraries
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                "~/Scripts/jquery-3.4.1.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                "~/Scripts/jquery-ui-1.12.1.js"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                "~/Scripts/bootstrap.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                "~/Scripts/jquery.validate.js",
                "~/Scripts/jquery.validate.unobtrusive.js"));

            // Page-level bundles (rendered from view sections)
            bundles.Add(new ScriptBundle("~/bundles/datatables").Include(
                "~/Scripts/DataTables/jquery.dataTables.js"));

            bundles.Add(new ScriptBundle("~/bundles/fancybox").Include(
                "~/Scripts/Fancybox/jquery.fancybox.js"));

            // Global AJAX setup + site behaviour (must come after jquery)
            bundles.Add(new ScriptBundle("~/bundles/site").Include(
                "~/Scripts/jquery.unobtrusive-ajax.js",
                "~/Scripts/site.js"));

            // Styles
            bundles.Add(new StyleBundle("~/Content/css").Include(
                "~/Content/bootstrap.css",
                "~/Content/themes/base/jquery-ui.css",
                "~/Content/site.css"));

            bundles.Add(new StyleBundle("~/Content/datatables").Include(
                "~/Content/DataTables/jquery.dataTables.css"));

            bundles.Add(new StyleBundle("~/Content/fancybox").Include(
                "~/Content/Fancybox/jquery.fancybox.css"));
        }
    }
}
