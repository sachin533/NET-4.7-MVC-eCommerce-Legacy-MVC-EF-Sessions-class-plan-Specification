namespace Ecommerce.Core.ViewModels
{
    /// <summary>
    /// Gallery image entry for the product detail Fancybox gallery.
    /// </summary>
    public class ProductImageViewModel
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public string AltText { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsPrimary { get; set; }
    }
}
