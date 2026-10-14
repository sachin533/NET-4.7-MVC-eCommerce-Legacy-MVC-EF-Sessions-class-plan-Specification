namespace Ecommerce.Core.Common
{
    /// <summary>
    /// Lightweight cart line stored in the session. Deliberately NOT an EF entity:
    /// the session only carries ids + quantity, product details are re-hydrated from the catalog.
    /// </summary>
    public class CartLine
    {
        public int ProductId { get; set; }
        public int? VariantId { get; set; }
        public int Quantity { get; set; }
    }
}
