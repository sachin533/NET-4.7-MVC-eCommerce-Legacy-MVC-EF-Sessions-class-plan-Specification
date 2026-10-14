using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Ecommerce.Core.Entities;
using Ecommerce.Core.Interfaces.Repositories;

namespace Ecommerce.Data
{
    public class CartRepository : ICartRepository
    {
        private readonly EcommerceDbContext _context;

        public CartRepository(EcommerceDbContext context)
        {
            _context = context;
        }

        public IList<CartItem> GetByCartId(string cartId)
        {
            return _context.CartItems
                .Include(c => c.Product)
                .Include(c => c.ProductVariant)
                .Where(c => c.CartId == cartId)
                .OrderBy(c => c.Id)
                .ToList();
        }

        public void Add(CartItem item)
        {
            _context.CartItems.Add(item);
        }

        public void Update(CartItem item)
        {
            _context.Entry(item).State = EntityState.Modified;
        }

        public void Remove(CartItem item)
        {
            _context.CartItems.Remove(item);
        }

        public void Clear(string cartId)
        {
            List<CartItem> rows = _context.CartItems.Where(c => c.CartId == cartId).ToList();
            _context.CartItems.RemoveRange(rows);
        }
    }
}
