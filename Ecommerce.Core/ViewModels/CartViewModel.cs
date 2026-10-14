using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Core.ViewModels
{
    /// <summary>
    /// Full cart page state (also returned by the AJAX add/update/remove actions so the
    /// client can refresh rows and totals without a round-trip to the cart page).
    /// </summary>
    public class CartViewModel
    {
        public CartViewModel()
        {
            Lines = new List<CartLineViewModel>();
        }

        public IList<CartLineViewModel> Lines { get; set; }

        [Display(Name = "Items")]
        public int ItemCount { get; set; }

        [Display(Name = "Subtotal")]
        public decimal SubTotal { get; set; }

        public bool HasItems { get { return Lines != null && Lines.Count > 0; } }

        /// <summary>True when the last add/update/remove operation succeeded.</summary>
        public bool Success { get; set; }

        /// <summary>User-facing feedback for the AJAX add-to-cart / update / remove actions.</summary>
        public string StatusMessage { get; set; }
    }
}
