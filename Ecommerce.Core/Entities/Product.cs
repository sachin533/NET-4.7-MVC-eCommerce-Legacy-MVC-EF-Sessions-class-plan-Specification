using System;
using System.Collections.Generic;

namespace Ecommerce.Core.Entities
{
    /// <summary>
    /// Catalog product. Maps to dbo.Products (EF6 Database First / EDMX).
    /// </summary>
    public class Product
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public string Sku { get; set; }
        public string ShortDescription { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsActive { get; set; }
        public string ThumbnailUrl { get; set; }
        public DateTime CreatedUtc { get; set; }
        public DateTime? ModifiedUtc { get; set; }

        public virtual Category Category { get; set; }
        public virtual ICollection<ProductImage> Images { get; set; }
        public virtual ICollection<ProductVariant> Variants { get; set; }
        public virtual ICollection<CartItem> CartItems { get; set; }
        public virtual ICollection<OrderLine> OrderLines { get; set; }

        public Product()
        {
            Images = new List<ProductImage>();
            Variants = new List<ProductVariant>();
            CartItems = new List<CartItem>();
            OrderLines = new List<OrderLine>();
        }
    }
}
