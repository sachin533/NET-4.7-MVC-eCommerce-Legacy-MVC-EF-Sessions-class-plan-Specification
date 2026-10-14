using System;
using System.Linq;
using Ecommerce.Core.Entities;
using Ecommerce.Core.Interfaces.Repositories;
using Ecommerce.Core.Interfaces.Services;
using Ecommerce.Core.ViewModels;

namespace Ecommerce.Services
{
    /// <summary>
    /// Customer-profile side of accounts. Registration/login itself is handled by
    /// ASP.NET Identity 2.0 (OWIN) in Ecommerce.Web; this service links the Identity user
    /// id to a Customers row and serves order history for the signed-in user only.
    /// </summary>
    public class AccountService : IAccountService
    {
        private readonly ICustomerRepository _customers;
        private readonly IOrderRepository _orders;
        private readonly IUnitOfWork _unitOfWork;

        public AccountService(ICustomerRepository customers, IOrderRepository orders, IUnitOfWork unitOfWork)
        {
            _customers = customers;
            _orders = orders;
            _unitOfWork = unitOfWork;
        }

        public int EnsureCustomerProfile(string userId, string email, string fullName)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("An Identity user id is required.", "userId");
            }

            var customer = _customers.GetByUserId(userId);
            if (customer != null) return customer.Id;

            customer = new Customer
            {
                UserId = userId,
                Email = string.IsNullOrWhiteSpace(email) ? string.Empty : email,
                FullName = string.IsNullOrWhiteSpace(fullName)
                    ? (string.IsNullOrWhiteSpace(email) ? "Customer" : email)
                    : fullName,
                CreatedUtc = DateTime.UtcNow
            };

            _customers.Add(customer);
            _unitOfWork.Complete();

            return customer.Id;
        }

        public CheckoutAddressViewModel GetDefaultShippingAddress(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return null;

            var customer = _customers.GetByUserId(userId);
            if (customer == null) return null;

            var address = _customers.GetDefaultAddress(customer.Id);
            if (address == null) return null;

            return new CheckoutAddressViewModel
            {
                Label = address.Label,
                FullName = string.IsNullOrWhiteSpace(address.FullName) ? customer.FullName : address.FullName,
                Email = customer.Email,
                Phone = address.Phone,
                Line1 = address.Line1,
                Line2 = address.Line2,
                City = address.City,
                State = address.State,
                PostalCode = address.PostalCode,
                Country = address.Country
            };
        }

        public OrderHistoryViewModel GetOrderHistory(string userId)
        {
            var vm = new OrderHistoryViewModel();

            if (string.IsNullOrEmpty(userId)) return vm;

            var customer = _customers.GetByUserId(userId);
            if (customer != null)
            {
                vm.FullName = customer.FullName;
                vm.Email = customer.Email;
            }

            foreach (var order in _orders.GetByUserId(userId))
            {
                vm.Orders.Add(Mapping.EntityMapper.ToOrderSummary(order));
            }

            return vm;
        }
    }
}
