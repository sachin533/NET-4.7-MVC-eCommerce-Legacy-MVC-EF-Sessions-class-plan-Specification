namespace Ecommerce.Core.Entities
{
    /// <summary>
    /// Gallery image for a product (dbo.ProductImages). The seed creates one primary image
    /// per product; additional rows drive the Fancybox gallery on the detail page.
    /// </summary>
    public class ProductImage
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string Url { get; set; }
        public string AltText { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsPrimary { get; set; }

        public virtual Product Product { get; set; }
    }
}
