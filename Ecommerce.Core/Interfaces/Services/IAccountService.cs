using Ecommerce.Core.ViewModels;

namespace Ecommerce.Core.Interfaces.Services
{
    /// <summary>
    /// Customer-profile side of accounts: links an Identity user to a Customer row and
    /// serves order history. Registration/login themselves are handled by ASP.NET Identity (OWIN).
    /// </summary>
    public interface IAccountService
    {
        /// <summary>Creates (or returns) the Customer profile for an Identity user id.</summary>
        /// <returns>The Customers.Id of the profile (created or existing).</returns>
        int EnsureCustomerProfile(string userId, string email, string fullName);

        /// <summary>The customer's saved shipping address (checkout step 1 prefill), or null.</summary>
        CheckoutAddressViewModel GetDefaultShippingAddress(string userId);

        OrderHistoryViewModel GetOrderHistory(string userId);
    }
}
