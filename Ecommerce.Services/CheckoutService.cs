using System;
using System.Linq;
using Ecommerce.Core.Entities;
using Ecommerce.Core.Interfaces.Repositories;
using Ecommerce.Core.Interfaces.Services;
using Ecommerce.Core.ViewModels;

namespace Ecommerce.Services
{
    /// <summary>
    /// Three-step checkout: Address -> Shipping -> Payment -> confirmation.
    ///
    /// SECURITY / BUSINESS RULES ENFORCED HERE:
    ///  * Every money value is recomputed from the catalog with IPricingCalculator. Nothing
    ///    sent by the browser (price, subtotal, tax, total) is ever trusted.
    ///  * Card data is used only for format validation. It is NEVER persisted, never logged
    ///    and never put in session: a locally generated DEMO reference is recorded instead.
    ///  * The order and its lines are a snapshot; the shipping address is copied onto the
    ///    order so later profile edits cannot rewrite order history.
    /// </summary>
    public class CheckoutService : ICheckoutService
    {
        private readonly ICartService _cartService;
        private readonly IPricingCalculator _pricing;
        private readonly IAccountService _accountService;
        private readonly ICustomerRepository _customers;
        private readonly IOrderRepository _orders;
        private readonly IUnitOfWork _unitOfWork;

        public CheckoutService(
            ICartService cartService,
            IPricingCalculator pricing,
            IAccountService accountService,
            ICustomerRepository customers,
            IOrderRepository orders,
            IUnitOfWork unitOfWork)
        {
            _cartService = cartService;
            _pricing = pricing;
            _accountService = accountService;
            _customers = customers;
            _orders = orders;
            _unitOfWork = unitOfWork;
        }

        public CheckoutShippingViewModel GetShippingStep(string selectedMethod)
        {
            var cart = _cartService.GetCart();
            if (!cart.HasItems)
            {
                throw new InvalidOperationException("Your cart is empty - add a product before checking out.");
            }

            var options = _pricing.GetShippingOptions();
            var selected = options.FirstOrDefault(o =>
                               string.Equals(o.Code, selectedMethod, StringComparison.OrdinalIgnoreCase))
                           ?? options.First();

            var totals = _pricing.ComputeTotals(cart.SubTotal, selected.Code);

            return new CheckoutShippingViewModel
            {
                ShippingMethod = selected.Code,
                ShippingOptions = options,
                SubTotal = totals.SubTotal,
                ShippingCost = totals.ShippingCost,
                Tax = totals.Tax,
                Total = totals.Total
            };
        }

        public OrderConfirmationViewModel PlaceOrder(
            CheckoutAddressViewModel address,
            CheckoutShippingViewModel shipping,
            CheckoutPaymentViewModel payment,
            string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new InvalidOperationException("You must be signed in to place an order.");
            }
            if (address == null)
            {
                throw new InvalidOperationException("Checkout step 1 (address) is incomplete.");
            }
            if (shipping == null || string.IsNullOrWhiteSpace(shipping.ShippingMethod))
            {
                throw new InvalidOperationException("Checkout step 2 (shipping method) is incomplete.");
            }
            if (payment == null)
            {
                throw new InvalidOperationException("Checkout step 3 (payment) is incomplete.");
            }

            // Re-validate the shipping method against the server-side option list.
            var option = _pricing.GetShippingOptions().FirstOrDefault(o =>
                string.Equals(o.Code, shipping.ShippingMethod.Trim(), StringComparison.OrdinalIgnoreCase));
            if (option == null)
            {
                throw new InvalidOperationException("Unknown shipping method. Please choose a valid option.");
            }

            var cart = _cartService.GetCart();
            if (!cart.HasItems)
            {
                throw new InvalidOperationException("Your cart is empty - there is nothing to order.");
            }

            // --- Totals are ALWAYS computed here, never taken from the client -------------
            var totals = _pricing.ComputeTotals(cart.SubTotal, option.Code);

            // --- Customer profile + first saved address -----------------------------------
            var customerId = _accountService.EnsureCustomerProfile(userId, address.Email, address.FullName);

            if (_customers.GetDefaultAddress(customerId) == null)
            {
                _customers.AddAddress(new Address
                {
                    CustomerId = customerId,
                    Label = string.IsNullOrWhiteSpace(address.Label) ? "Shipping" : address.Label,
                    FullName = address.FullName,
                    Line1 = address.Line1,
                    Line2 = address.Line2,
                    City = address.City,
                    State = address.State,
                    PostalCode = address.PostalCode,
                    Country = address.Country,
                    Phone = address.Phone,
                    IsDefault = true
                });
            }

            // --- Order + snapshotted lines --------------------------------------------------
            var order = new Order
            {
                OrderNumber = GenerateOrderNumber(),
                UserId = userId,
                CustomerId = customerId,
                OrderDateUtc = DateTime.UtcNow,
                Status = "Pending",
                SubTotal = totals.SubTotal,
                ShippingCost = totals.ShippingCost,
                Tax = totals.Tax,
                Total = totals.Total,
                Currency = "USD",
                ShippingMethod = option.DisplayName,
                PaymentMethod = string.IsNullOrWhiteSpace(payment.PaymentMethod) ? "Card" : payment.PaymentMethod,
                PaymentReference = GenerateDemoPaymentReference(),
                Notes = "Demo checkout - no payment gateway was contacted and no payment was processed.",
                ShipFullName = address.FullName,
                ShipLine1 = address.Line1,
                ShipLine2 = address.Line2,
                ShipCity = address.City,
                ShipState = address.State,
                ShipPostalCode = address.PostalCode,
                ShipCountry = address.Country,
                ShipPhone = address.Phone
            };

            foreach (var line in cart.Lines)
            {
                order.OrderLines.Add(new OrderLine
                {
                    ProductId = line.ProductId,
                    ProductName = line.ProductName,
                    Sku = line.Sku,
                    VariantName = line.VariantName,
                    UnitPrice = line.UnitPrice,
                    Quantity = line.Quantity,
                    LineTotal = line.LineTotal
                });
            }

            _orders.Add(order);
            _unitOfWork.Complete();

            _cartService.ClearCart();

            return new OrderConfirmationViewModel
            {
                OrderNumber = order.OrderNumber,
                OrderDateUtc = order.OrderDateUtc,
                Total = order.Total,
                ItemCount = order.OrderLines.Sum(l => l.Quantity),
                ShippingMethod = order.ShippingMethod,
                PaymentReference = order.PaymentReference
            };
        }

        private static string GenerateOrderNumber()
        {
            // e.g. LEG-20261013-4F3A9C  (fits Orders.OrderNumber NVARCHAR(30) + UNIQUE)
            return "LEG-" + DateTime.UtcNow.ToString("yyyyMMdd") + "-" +
                   Guid.NewGuid().ToString("N").Substring(0, 6).ToUpperInvariant();
        }

        private static string GenerateDemoPaymentReference()
        {
            // Local-only reference. No gateway, no card data, nothing transmitted.
            return "DEMO-" + DateTime.UtcNow.ToString("yyyyMMddHHmmss") + "-" +
                   Guid.NewGuid().ToString("N").Substring(0, 8).ToUpperInvariant();
        }
    }
}
