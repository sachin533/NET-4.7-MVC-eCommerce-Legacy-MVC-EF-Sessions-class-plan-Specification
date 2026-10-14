using System;
using System.Collections.Generic;

namespace Ecommerce.Core.ViewModels
{
    /// <summary>
    /// Product detail page. Deliberately exposes scalar values + nested VMs only —
    /// the spec forbids leaking EDMX entities into views (no `Product` entity property here).
    /// </summary>
    public class ProductDetailViewModel
    {
        public ProductDetailViewModel()
        {
            Images = new List<ProductImageViewModel>();
            Variants = new List<ProductVariantViewModel>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public string Sku { get; set; }
        public string ShortDescription { get; set; }
        public string Description { get; set; }

        /// <summary>Base price (variants add PriceAdjustment on top).</summary>
        public decimal Price { get; set; }

        public int Stock { get; set; }
        public bool IsActive { get; set; }
        public string ThumbnailUrl { get; set; }

        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public DateTime CreatedUtc { get; set; }

        /// <summary>Selected variant for the add-to-cart form (bound by DropDownListFor).</summary>
        public int? SelectedVariantId { get; set; }

        /// <summary>Quantity for the add-to-cart form.</summary>
        public int Quantity { get; set; }

        public IList<ProductImageViewModel> Images { get; set; }
        public IList<ProductVariantViewModel> Variants { get; set; }
    }
}
