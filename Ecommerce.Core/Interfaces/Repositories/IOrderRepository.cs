using System.Collections.Generic;
using Ecommerce.Core.Entities;

namespace Ecommerce.Core.Interfaces.Repositories
{
    public interface IOrderRepository
    {
        void Add(Order order);

        /// <summary>Single order with OrderLines + Customer eagerly loaded.</summary>
        Order GetById(int id);

        /// <summary>A user's orders, newest first, with OrderLines eagerly loaded.</summary>
        IList<Order> GetByUserId(string userId);
    }
}
