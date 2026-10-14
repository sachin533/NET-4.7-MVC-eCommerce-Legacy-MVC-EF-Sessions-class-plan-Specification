using Ecommerce.Core.ViewModels;

namespace Ecommerce.Core.Interfaces.Services
{
    /// <summary>
    /// Session cart: reads/writes cart lines through ISessionCartStore, validates against
    /// the catalog and write-throughs CartItem rows so a cart survives session expiry.
    /// </summary>
    public interface ICartService
    {
        CartViewModel GetCart();

        MiniCartViewModel GetMiniCart();

        CartViewModel AddItem(int productId, int? variantId, int quantity);

        CartViewModel UpdateQuantity(int productId, int? variantId, int quantity);

        CartViewModel RemoveItem(int productId, int? variantId);

        /// <summary>Empties session lines + persisted rows (called after checkout).</summary>
        void ClearCart();
    }
}
