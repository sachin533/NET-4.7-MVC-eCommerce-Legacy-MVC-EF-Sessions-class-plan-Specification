using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Core.ViewModels
{
    /// <summary>
    /// Checkout step 3 — payment. DEMO ONLY: no gateway integration, nothing is transmitted
    /// or persisted beyond a locally generated reference (see ICheckoutService).
    /// </summary>
    public class CheckoutPaymentViewModel
    {
        [Required]
        [StringLength(150)]
        [Display(Name = "Cardholder name")]
        public string CardholderName { get; set; }

        [Required]
        [RegularExpression(@"^[0-9 ]{15,19}$", ErrorMessage = "Enter a valid 15-19 digit card number.")]
        [Display(Name = "Card number")]
        public string CardNumber { get; set; }

        [Required]
        [Range(1, 12)]
        [Display(Name = "Expiry month")]
        public int ExpiryMonth { get; set; }

        [Required]
        [Range(2026, 2040)]
        [Display(Name = "Expiry year")]
        public int ExpiryYear { get; set; }

        [Required]
        [RegularExpression(@"^[0-9]{3,4}$", ErrorMessage = "Enter a valid CVV.")]
        [Display(Name = "CVV")]
        public string Cvc { get; set; }

        [Display(Name = "Payment method")]
        public string PaymentMethod { get; set; }

        public CheckoutPaymentViewModel()
        {
            PaymentMethod = "Card";
            ExpiryYear = 2026;
        }
    }
}
