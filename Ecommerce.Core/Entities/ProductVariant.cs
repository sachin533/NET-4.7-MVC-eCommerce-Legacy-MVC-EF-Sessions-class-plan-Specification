using System.Collections.Generic;

namespace Ecommerce.Core.Entities
{
    /// <summary>
    /// Product variant (colour / configuration). PriceAdjustment is added to the
    /// base product price to produce the effective unit price.
    /// </summary>
    public class ProductVariant
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string Sku { get; set; }
        public decimal PriceAdjustment { get; set; }
        public int Stock { get; set; }
        public bool IsActive { get; set; }

        public virtual Product Product { get; set; }
        public virtual ICollection<CartItem> CartItems { get; set; }

        public ProductVariant()
        {
            CartItems = new List<CartItem>();
        }
    }
}
