using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Core.ViewModels
{
    /// <summary>
    /// One row of the cart page (and of the MiniCart projection).
    /// </summary>
    public class CartLineViewModel
    {
        public int ProductId { get; set; }
        public int? VariantId { get; set; }

        [Display(Name = "Product")]
        public string ProductName { get; set; }

        [Display(Name = "Variant")]
        public string VariantName { get; set; }

        public string Sku { get; set; }
        public string ThumbnailUrl { get; set; }

        [Display(Name = "Unit price")]
        public decimal UnitPrice { get; set; }

        [Display(Name = "Qty")]
        public int Quantity { get; set; }

        [Display(Name = "Line total")]
        public decimal LineTotal { get; set; }

        /// <summary>Stock available for this product/variant (for client-side qty clamping).</summary>
        public int AvailableStock { get; set; }
    }
}
