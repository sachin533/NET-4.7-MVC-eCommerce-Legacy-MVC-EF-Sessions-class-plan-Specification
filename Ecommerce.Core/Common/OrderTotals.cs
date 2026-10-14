using System;

namespace Ecommerce.Core.Common
{
    /// <summary>
    /// Computed money totals for a cart / order (produced by IPricingCalculator).
    /// </summary>
    public class OrderTotals
    {
        public decimal SubTotal { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal Tax { get; set; }
        public decimal Total { get; set; }

        public static OrderTotals Zero
        {
            get { return new OrderTotals(); }
        }

        public static OrderTotals operator +(OrderTotals a, OrderTotals b)
        {
            if (a == null || b == null) throw new ArgumentNullException();
            return new OrderTotals
            {
                SubTotal = a.SubTotal + b.SubTotal,
                ShippingCost = a.ShippingCost + b.ShippingCost,
                Tax = a.Tax + b.Tax,
                Total = a.Total + b.Total
            };
        }
    }
}
