using Ecommerce.Core.ViewModels;

namespace Ecommerce.Core.Interfaces.Services
{
    /// <summary>
    /// Three-step checkout: address → shipping → payment. The controller holds the step
    /// ViewModels in session; this service persists customer/address/order + snapshots.
    /// </summary>
    public interface ICheckoutService
    {
        /// <summary>Shipping options + totals for step 2 (optionally pre-selecting a method).</summary>
        CheckoutShippingViewModel GetShippingStep(string selectedMethod);

        /// <summary>
        /// Step 3: builds the order from the current cart, snapshots the address, records a
        /// local (demo) payment reference and clears the cart. Throws InvalidOperationException
        /// when the cart is empty or the shipping step is incomplete.
        /// </summary>
        OrderConfirmationViewModel PlaceOrder(
            CheckoutAddressViewModel address,
            CheckoutShippingViewModel shipping,
            CheckoutPaymentViewModel payment,
            string userId);
    }
}
