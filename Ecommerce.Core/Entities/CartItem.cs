using System;
using System.Collections.Generic;

namespace Ecommerce.Core.Entities
{
    /// <summary>
    /// Persisted cart row (dbo.CartItems). The session holds the authoritative shopping
    /// session; these rows survive session expiry so an anonymous cart can be restored.
    /// </summary>
    public class CartItem
    {
        public int Id { get; set; }
        public string CartId { get; set; }
        public string UserId { get; set; }
        public int ProductId { get; set; }
        public int? VariantId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public DateTime CreatedUtc { get; set; }
        public DateTime? ModifiedUtc { get; set; }

        public virtual Product Product { get; set; }
        public virtual ProductVariant ProductVariant { get; set; }

        public CartItem()
        {
        }

        public CartItem(string cartId, int productId, int? variantId, int quantity, decimal unitPrice)
        {
            CartId = cartId;
            ProductId = productId;
            VariantId = variantId;
            Quantity = quantity;
            UnitPrice = unitPrice;
            CreatedUtc = DateTime.UtcNow;
        }
    }
}
