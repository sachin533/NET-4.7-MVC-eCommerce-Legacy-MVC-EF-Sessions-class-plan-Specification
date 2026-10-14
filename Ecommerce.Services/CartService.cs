using System;
using System.Collections.Generic;
using System.Linq;
using Ecommerce.Core.Common;
using Ecommerce.Core.Entities;
using Ecommerce.Core.Interfaces;
using Ecommerce.Core.Interfaces.Repositories;
using Ecommerce.Core.Interfaces.Services;
using Ecommerce.Core.ViewModels;

namespace Ecommerce.Services
{
    /// <summary>
    /// Session cart. The session (ISessionCartStore, implemented in Ecommerce.Web) is the
    /// authoritative source of "what the visitor put in the cart"; it only stores ids + qty.
    /// Every price is re-hydrated from the catalog here - never from the client - and the
    /// CartItems table is written through so the cart survives session expiry.
    /// </summary>
    public class CartService : ICartService
    {
        private readonly ISessionCartStore _store;
        private readonly IProductRepository _products;
        private readonly ICartRepository _cartRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CartService(
            ISessionCartStore store,
            IProductRepository products,
            ICartRepository cartRepository,
            IUnitOfWork unitOfWork)
        {
            _store = store;
            _products = products;
            _cartRepository = cartRepository;
            _unitOfWork = unitOfWork;
        }

        public CartViewModel GetCart()
        {
            return LoadCart();
        }

        public MiniCartViewModel GetMiniCart()
        {
            var cart = LoadCart();
            return new MiniCartViewModel
            {
                ItemCount = cart.ItemCount,
                SubTotal = cart.SubTotal
            };
        }

        public CartViewModel AddItem(int productId, int? variantId, int quantity)
        {
            var lines = _store.GetLines() ?? new List<CartLine>();
            var amount = quantity < 1 ? 1 : quantity;

            var existing = lines.FirstOrDefault(l => l.ProductId == productId && l.VariantId == variantId);
            if (existing != null)
            {
                existing.Quantity += amount;
            }
            else
            {
                lines.Add(new CartLine { ProductId = productId, VariantId = variantId, Quantity = amount });
            }

            _store.SetLines(lines);

            var cart = LoadCart();
            Persist(cart);

            cart.Success = cart.Lines.Any(l => l.ProductId == productId && l.VariantId == variantId);
            cart.StatusMessage = cart.Success
                ? "Added to your cart."
                : "Sorry - that item is no longer available.";

            return cart;
        }

        public CartViewModel UpdateQuantity(int productId, int? variantId, int quantity)
        {
            if (quantity <= 0)
            {
                return RemoveItem(productId, variantId);
            }

            var lines = _store.GetLines() ?? new List<CartLine>();
            var line = lines.FirstOrDefault(l => l.ProductId == productId && l.VariantId == variantId);
            if (line != null)
            {
                line.Quantity = quantity;
                _store.SetLines(lines);
            }

            var cart = LoadCart();
            Persist(cart);

            cart.Success = line != null;
            cart.StatusMessage = line != null ? "Cart updated." : "That item is no longer in your cart.";

            return cart;
        }

        public CartViewModel RemoveItem(int productId, int? variantId)
        {
            var lines = _store.GetLines() ?? new List<CartLine>();
            var removed = false;
            for (var i = lines.Count - 1; i >= 0; i--)
            {
                var candidate = lines[i];
                if (candidate == null || candidate.ProductId != productId || candidate.VariantId != variantId) continue;
                lines.RemoveAt(i);
                removed = true;
            }
            _store.SetLines(lines);

            var cart = LoadCart();
            Persist(cart);

            cart.Success = true;
            cart.StatusMessage = removed ? "Item removed from your cart." : "That item was not in your cart.";

            return cart;
        }

        public void ClearCart()
        {
            _store.ClearLines();
            _cartRepository.Clear(_store.GetOrCreateCartId());
            _unitOfWork.Complete();
        }

        /// <summary>
        /// Hydrates the session lines from the catalog, clamps quantities to the real stock,
        /// drops lines that are no longer purchasable and writes the session back if it changed.
        /// </summary>
        private CartViewModel LoadCart()
        {
            var requested = _store.GetLines() ?? new List<CartLine>();
            var cart = new CartViewModel { Success = true };
            var valid = new List<CartLine>();

            foreach (var line in requested)
            {
                if (line == null || line.ProductId <= 0 || line.Quantity <= 0) continue;

                var product = _products.GetById(line.ProductId);
                if (product == null || !product.IsActive) continue;

                ProductVariant variant = null;
                if (line.VariantId.HasValue)
                {
                    variant = product.Variants.FirstOrDefault(v => v.Id == line.VariantId.Value && v.IsActive);
                    if (variant == null) continue;
                }

                var stock = variant != null ? variant.Stock : product.Stock;
                if (stock <= 0) continue;

                var quantity = line.Quantity > stock ? stock : line.Quantity;
                var unitPrice = product.Price + (variant != null ? variant.PriceAdjustment : 0m);

                valid.Add(new CartLine
                {
                    ProductId = product.Id,
                    VariantId = line.VariantId,
                    Quantity = quantity
                });

                cart.Lines.Add(new CartLineViewModel
                {
                    ProductId = product.Id,
                    VariantId = line.VariantId,
                    ProductName = product.Name,
                    VariantName = variant != null ? variant.Name : null,
                    Sku = variant != null ? variant.Sku : product.Sku,
                    ThumbnailUrl = string.IsNullOrWhiteSpace(product.ThumbnailUrl)
                        ? Mapping.EntityMapper.PlaceholderImage
                        : product.ThumbnailUrl,
                    UnitPrice = unitPrice,
                    Quantity = quantity,
                    LineTotal = unitPrice * quantity,
                    AvailableStock = stock
                });
            }

            cart.ItemCount = cart.Lines.Sum(l => l.Quantity);
            cart.SubTotal = cart.Lines.Sum(l => l.LineTotal);

            if (!SameAs(requested, valid))
            {
                _store.SetLines(valid);
            }

            return cart;
        }

        /// <summary>Write-through: mirrors the session cart onto the CartItems rows.</summary>
        private void Persist(CartViewModel cart)
        {
            var cartId = _store.GetOrCreateCartId();
            var userId = _store.UserId;
            var existing = _cartRepository.GetByCartId(cartId);

            foreach (var row in existing)
            {
                var keep = cart.Lines.Any(l => l.ProductId == row.ProductId && l.VariantId == row.VariantId);
                if (!keep)
                {
                    _cartRepository.Remove(row);
                }
            }

            foreach (var line in cart.Lines)
            {
                var row = existing.FirstOrDefault(r => r.ProductId == line.ProductId && r.VariantId == line.VariantId);
                if (row == null)
                {
                    var created = new CartItem(cartId, line.ProductId, line.VariantId, line.Quantity, line.UnitPrice)
                    {
                        UserId = userId,
                        ModifiedUtc = DateTime.UtcNow
                    };
                    _cartRepository.Add(created);
                }
                else if (row.Quantity != line.Quantity || row.UnitPrice != line.UnitPrice || row.UserId != userId)
                {
                    row.Quantity = line.Quantity;
                    row.UnitPrice = line.UnitPrice;
                    row.UserId = userId;
                    row.ModifiedUtc = DateTime.UtcNow;
                    _cartRepository.Update(row);
                }
            }

            _unitOfWork.Complete();
        }

        private static bool SameAs(IList<CartLine> requested, IList<CartLine> valid)
        {
            if (requested.Count != valid.Count) return false;

            for (var i = 0; i < valid.Count; i++)
            {
                var a = requested.FirstOrDefault(r => r != null && r.ProductId == valid[i].ProductId && r.VariantId == valid[i].VariantId);
                if (a == null || a.Quantity != valid[i].Quantity) return false;
            }

            return true;
        }
    }
}
