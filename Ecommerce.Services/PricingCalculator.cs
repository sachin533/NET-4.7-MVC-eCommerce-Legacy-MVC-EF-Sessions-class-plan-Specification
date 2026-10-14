using System;
using System.Collections.Generic;
using Ecommerce.Core.Common;
using Ecommerce.Core.Interfaces.Services;
using Ecommerce.Core.ViewModels;

namespace Ecommerce.Services
{
    /// <summary>
    /// Single source of truth for the demo pricing rules.
    /// Shipping and tax are computed SERVER-SIDE only: no price or total coming from the
    /// browser is ever trusted (see CheckoutService.PlaceOrder).
    /// </summary>
    public class PricingCalculator : IPricingCalculator
    {
        /// <summary>Demo tax rate (percent) applied to the subtotal.</summary>
        public decimal TaxRate
        {
            get { return 8.25m; }
        }

        public IList<ShippingOptionViewModel> GetShippingOptions()
        {
            return new List<ShippingOptionViewModel>
            {
                new ShippingOptionViewModel
                {
                    Code = "standard",
                    DisplayName = "Standard delivery",
                    Cost = 0.00m,
                    DeliveryEstimate = "5-7 business days"
                },
                new ShippingOptionViewModel
                {
                    Code = "express",
                    DisplayName = "Express delivery",
                    Cost = 12.99m,
                    DeliveryEstimate = "2-3 business days"
                },
                new ShippingOptionViewModel
                {
                    Code = "overnight",
                    DisplayName = "Overnight priority",
                    Cost = 24.99m,
                    DeliveryEstimate = "Next business day"
                }
            };
        }

        public decimal GetShippingCost(string shippingMethod)
        {
            if (string.IsNullOrWhiteSpace(shippingMethod))
            {
                return GetShippingOptions()[0].Cost;
            }

            foreach (var option in GetShippingOptions())
            {
                if (string.Equals(option.Code, shippingMethod.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    return option.Cost;
                }
            }

            throw new ArgumentException(
                "Unknown shipping method '" + shippingMethod + "'. Valid values: standard, express, overnight.",
                "shippingMethod");
        }

        public OrderTotals ComputeTotals(decimal subTotal, string shippingMethod)
        {
            if (subTotal < 0m) throw new ArgumentOutOfRangeException("subTotal");

            var shipping = GetShippingCost(shippingMethod);
            var tax = Math.Round(subTotal * (TaxRate / 100m), 2, MidpointRounding.AwayFromZero);

            return new OrderTotals
            {
                SubTotal = subTotal,
                ShippingCost = shipping,
                Tax = tax,
                Total = subTotal + shipping + tax
            };
        }
    }
}
