using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Core.ViewModels
{
    /// <summary>
    /// Checkout step 2 — delivery method + the running totals summary.
    /// </summary>
    public class CheckoutShippingViewModel
    {
        public CheckoutShippingViewModel()
        {
            ShippingOptions = new List<ShippingOptionViewModel>();
        }

        [Required(ErrorMessage = "Please choose a shipping method.")]
        [Display(Name = "Shipping method")]
        public string ShippingMethod { get; set; }

        public IList<ShippingOptionViewModel> ShippingOptions { get; set; }

        [Display(Name = "Subtotal")]
        public decimal SubTotal { get; set; }

        [Display(Name = "Shipping")]
        public decimal ShippingCost { get; set; }

        [Display(Name = "Tax")]
        public decimal Tax { get; set; }

        [Display(Name = "Total")]
        public decimal Total { get; set; }
    }

    /// <summary>
    /// One selectable shipping option (costs come from IPricingCalculator — demo values).
    /// </summary>
    public class ShippingOptionViewModel
    {
        public string Code { get; set; }
        public string DisplayName { get; set; }
        public decimal Cost { get; set; }
        public string DeliveryEstimate { get; set; }
    }
}
