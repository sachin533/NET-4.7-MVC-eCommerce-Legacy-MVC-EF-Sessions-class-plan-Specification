using System.Collections.Generic;

namespace Ecommerce.Core.ViewModels
{
    /// <summary>
    /// Order history page (Account / Orders).
    /// </summary>
    public class OrderHistoryViewModel
    {
        public OrderHistoryViewModel()
        {
            Orders = new List<OrderSummaryViewModel>();
        }

        public IList<OrderSummaryViewModel> Orders { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
    }
}
