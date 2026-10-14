using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Core.ViewModels
{
    /// <summary>
    /// Selectable product variant. EffectivePrice = base Price + PriceAdjustment.
    /// </summary>
    public class ProductVariantViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Option")]
        public string Name { get; set; }

        public string Sku { get; set; }
        public decimal PriceAdjustment { get; set; }
        public int Stock { get; set; }
        public bool IsActive { get; set; }
    }
}
