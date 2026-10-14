using System.Linq;
using Ecommerce.Core.Entities;
using Ecommerce.Core.ViewModels;

namespace Ecommerce.Services.Mapping
{
    /// <summary>
    /// Central entity-to-ViewModel mapping. Views bind to ViewModels only — never EF entities.
    /// </summary>
    public static class EntityMapper
    {
        public const string PlaceholderImage = "/Content/images/products/placeholder.png";

        public static string ResolveImage(string url)
        {
            return string.IsNullOrWhiteSpace(url) ? PlaceholderImage : url;
        }

        public static ProductCardViewModel ToCard(Product p)
        {
            return new ProductCardViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Slug = p.Slug,
                Sku = p.Sku,
                Price = p.Price,
                ThumbnailUrl = ResolveImage(p.ThumbnailUrl),
                ShortDescription = p.ShortDescription,
                CategoryName = p.Category != null ? p.Category.Name : null,
                IsFeatured = p.IsFeatured,
                Stock = p.Stock
            };
        }

        public static ProductDetailViewModel ToDetail(Product p)
        {
            var vm = new ProductDetailViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Slug = p.Slug,
                Sku = p.Sku,
                ShortDescription = p.ShortDescription,
                Description = p.Description,
                Price = p.Price,
                Stock = p.Stock,
                IsActive = p.IsActive,
                ThumbnailUrl = ResolveImage(p.ThumbnailUrl),
                CategoryId = p.CategoryId,
                CategoryName = p.Category != null ? p.Category.Name : null,
                CreatedUtc = p.CreatedUtc
            };

            if (p.Images != null)
            {
                vm.Images = p.Images
                    .OrderBy(i => i.DisplayOrder)
                    .Select(i => new ProductImageViewModel
                    {
                        Id = i.Id,
                        Url = ResolveImage(i.Url),
                        AltText = i.AltText ?? p.Name,
                        DisplayOrder = i.DisplayOrder,
                        IsPrimary = i.IsPrimary
                    }).ToList();
            }

            if (p.Variants != null)
            {
                vm.Variants = p.Variants
                    .Where(v => v.IsActive)
                    .Select(v => new ProductVariantViewModel
                    {
                        Id = v.Id,
                        Name = v.Name,
                        Sku = v.Sku,
                        PriceAdjustment = v.PriceAdjustment,
                        Stock = v.Stock,
                        IsActive = v.IsActive
                    }).ToList();
            }

            return vm;
        }

        public static OrderSummaryViewModel ToOrderSummary(Order o)
        {
            return new OrderSummaryViewModel
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                OrderDateUtc = o.OrderDateUtc,
                Status = o.Status,
                ItemCount = o.OrderLines != null ? o.OrderLines.Sum(l => l.Quantity) : 0,
                Total = o.Total,
                Currency = o.Currency
            };
        }
    }
}
