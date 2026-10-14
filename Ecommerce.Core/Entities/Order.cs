using System;
using System.Collections.Generic;

namespace Ecommerce.Core.Entities
{
    /// <summary>
    /// Placed order (dbo.Orders). The Ship* columns are a snapshot of the checkout address
    /// so later address edits never alter order history.
    /// </summary>
    public class Order
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; }
        public string UserId { get; set; }
        public int? CustomerId { get; set; }
        public DateTime OrderDateUtc { get; set; }
        public string Status { get; set; }
        public decimal SubTotal { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal Tax { get; set; }
        public decimal Total { get; set; }
        public string Currency { get; set; }
        public string ShippingMethod { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentReference { get; set; }
        public string Notes { get; set; }

        public string ShipFullName { get; set; }
        public string ShipLine1 { get; set; }
        public string ShipLine2 { get; set; }
        public string ShipCity { get; set; }
        public string ShipState { get; set; }
        public string ShipPostalCode { get; set; }
        public string ShipCountry { get; set; }
        public string ShipPhone { get; set; }

        public virtual Customer Customer { get; set; }
        public virtual ICollection<OrderLine> OrderLines { get; set; }

        public Order()
        {
            OrderLines = new List<OrderLine>();
        }
    }
}
