using System;

namespace Ecommerce.Core.Entities
{
    /// <summary>
    /// Order line with product snapshot values (name / sku / unit price) frozen at purchase time.
    /// </summary>
    public class OrderLine
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string Sku { get; set; }
        public string VariantName { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal LineTotal { get; set; }

        public virtual Order Order { get; set; }
        public virtual Product Product { get; set; }
    }
}
