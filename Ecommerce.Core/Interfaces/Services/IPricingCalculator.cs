using System.Collections.Generic;
using Ecommerce.Core.Common;
using Ecommerce.Core.ViewModels;

namespace Ecommerce.Core.Interfaces.Services
{
    /// <summary>
    /// Single source of truth for demo pricing rules: shipping costs, tax rate and totals.
    /// </summary>
    public interface IPricingCalculator
    {
        IList<ShippingOptionViewModel> GetShippingOptions();

        decimal GetShippingCost(string shippingMethod);

        /// <summary>Demo tax rate applied to the subtotal (percent).</summary>
        decimal TaxRate { get; }

        OrderTotals ComputeTotals(decimal subTotal, string shippingMethod);
    }
}
