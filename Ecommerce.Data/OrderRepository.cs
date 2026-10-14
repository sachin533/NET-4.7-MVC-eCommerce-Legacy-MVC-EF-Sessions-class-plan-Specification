using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Ecommerce.Core.Entities;
using Ecommerce.Core.Interfaces.Repositories;

namespace Ecommerce.Data
{
    public class OrderRepository : IOrderRepository
    {
        private readonly EcommerceDbContext _context;

        public OrderRepository(EcommerceDbContext context)
        {
            _context = context;
        }

        public void Add(Order order)
        {
            _context.Orders.Add(order);
        }

        public Order GetById(int id)
        {
            return _context.Orders
                .Include(o => o.OrderLines)
                .Include(o => o.Customer)
                .FirstOrDefault(o => o.Id == id);
        }

        public IList<Order> GetByUserId(string userId)
        {
            return _context.Orders
                .Include(o => o.OrderLines)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDateUtc)
                .ToList();
        }
    }
}
