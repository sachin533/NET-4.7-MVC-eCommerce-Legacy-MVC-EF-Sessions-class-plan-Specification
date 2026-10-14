using Ecommerce.Core.Entities;

namespace Ecommerce.Core.Interfaces.Repositories
{
    public interface ICustomerRepository
    {
        Customer GetByUserId(string userId);

        void Add(Customer customer);

        void AddAddress(Address address);

        /// <summary>The customer's default (or most recent) saved address, or null.</summary>
        Address GetDefaultAddress(int customerId);
    }
}
