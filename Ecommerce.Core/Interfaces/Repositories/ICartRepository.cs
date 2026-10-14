using System.Collections.Generic;
using Ecommerce.Core.Entities;

namespace Ecommerce.Core.Interfaces.Repositories
{
    public interface ICartRepository
    {
        /// <summary>Persisted cart rows for a cart id (session GUID or user id), product data included.</summary>
        IList<CartItem> GetByCartId(string cartId);

        void Add(CartItem item);

        void Update(CartItem item);

        void Remove(CartItem item);

        /// <summary>Removes every row of the given cart (used after a successful checkout).</summary>
        void Clear(string cartId);
    }
}
