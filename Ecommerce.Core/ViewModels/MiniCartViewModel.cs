using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Core.ViewModels
{
    /// <summary>
    /// Header mini-cart projection (item count + subtotal) refreshed via AJAX (.load).
    /// </summary>
    public class MiniCartViewModel
    {
        [Display(Name = "Items")]
        public int ItemCount { get; set; }

        [Display(Name = "Subtotal")]
        public decimal SubTotal { get; set; }
    }
}
