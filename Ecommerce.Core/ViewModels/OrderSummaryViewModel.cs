using System;
using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Core.ViewModels
{
    /// <summary>
    /// Compact order row for the Account / order-history page.
    /// </summary>
    public class OrderSummaryViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Order #")]
        public string OrderNumber { get; set; }

        [Display(Name = "Date (UTC)")]
        public DateTime OrderDateUtc { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; }

        [Display(Name = "Items")]
        public int ItemCount { get; set; }

        [Display(Name = "Total")]
        public decimal Total { get; set; }

        public string Currency { get; set; }
    }
}
