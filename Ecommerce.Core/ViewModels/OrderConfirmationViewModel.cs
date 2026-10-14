using System;
using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Core.ViewModels
{
    /// <summary>
    /// Checkout confirmation page (step 4 result).
    /// </summary>
    public class OrderConfirmationViewModel
    {
        [Display(Name = "Order number")]
        public string OrderNumber { get; set; }

        [Display(Name = "Placed on (UTC)")]
        public DateTime OrderDateUtc { get; set; }

        [Display(Name = "Total")]
        public decimal Total { get; set; }

        [Display(Name = "Items")]
        public int ItemCount { get; set; }

        [Display(Name = "Shipping method")]
        public string ShippingMethod { get; set; }

        [Display(Name = "Payment reference")]
        public string PaymentReference { get; set; }
    }
}
