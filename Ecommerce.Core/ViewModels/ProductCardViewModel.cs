using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Core.ViewModels
{
    /// <summary>
    /// Lightweight projection used by product cards, grids and the homepage carousel.
    /// </summary>
    public class ProductCardViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Product")]
        public string Name { get; set; }

        public string Slug { get; set; }
        public string Sku { get; set; }

        [Display(Name = "Price")]
        public decimal Price { get; set; }

        public string ThumbnailUrl { get; set; }
        public string ShortDescription { get; set; }
        public string CategoryName { get; set; }
        public bool IsFeatured { get; set; }
        public int Stock { get; set; }
    }
}
