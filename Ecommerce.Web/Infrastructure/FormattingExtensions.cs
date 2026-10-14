using System.Globalization;
using System.Web.Mvc;

namespace Ecommerce.Web.Infrastructure
{
    /// <summary>
    /// Small view helpers. Views bind ViewModels only — these helpers just format
    /// primitives already carried by those ViewModels (demo currency is always USD).
    /// </summary>
    public static class FormattingExtensions
    {
        /// <summary>$1,234.56 — deterministic regardless of server culture.</summary>
        public static string Money(this HtmlHelper html, decimal value)
        {
            return value.ToString("C2", CultureInfo.GetCultureInfo("en-US"));
        }

        /// <summary>Renders a thumbnail or a neutral placeholder when the product has no image.</summary>
        public static string ImageOrPlaceholder(this HtmlHelper html, string url, string alt)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return "/Content/images/placeholder.png";
            }
            return url;
        }
    }
}
