using System.Data.Entity;
using System.Linq;
using Ecommerce.Core.Entities;
using Ecommerce.Core.Interfaces.Repositories;

namespace Ecommerce.Data
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly EcommerceDbContext _context;

        public CustomerRepository(EcommerceDbContext context)
        {
            _context = context;
        }

        public Customer GetByUserId(string userId)
        {
            return _context.Customers.FirstOrDefault(c => c.UserId == userId);
        }

        public void Add(Customer customer)
        {
            _context.Customers.Add(customer);
        }

        public void AddAddress(Address address)
        {
            _context.Addresses.Add(address);
        }

        public Address GetDefaultAddress(int customerId)
        {
            return _context.Addresses
                .Where(a => a.CustomerId == customerId)
                .OrderByDescending(a => a.IsDefault)
                .ThenByDescending(a => a.Id)
                .FirstOrDefault();
        }
    }
}
