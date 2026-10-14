using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Core.ViewModels
{
    /// <summary>
    /// Checkout step 1 — shipping address. Posted values are snapshotted onto the order.
    /// </summary>
    public class CheckoutAddressViewModel
    {
        [Display(Name = "Label")]
        [StringLength(50)]
        public string Label { get; set; }

        [Required]
        [StringLength(150)]
        [Display(Name = "Full name")]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(256)]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Phone]
        [StringLength(30)]
        [Display(Name = "Phone")]
        public string Phone { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Address line 1")]
        public string Line1 { get; set; }

        [StringLength(200)]
        [Display(Name = "Address line 2")]
        public string Line2 { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "City")]
        public string City { get; set; }

        [StringLength(100)]
        [Display(Name = "State / Province")]
        public string State { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Postal code")]
        public string PostalCode { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Country")]
        public string Country { get; set; }
    }
}
