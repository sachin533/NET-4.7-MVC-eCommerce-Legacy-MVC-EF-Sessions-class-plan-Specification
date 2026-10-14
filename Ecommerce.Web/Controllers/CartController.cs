using System;
using System.Web.Mvc;
using Ecommerce.Core.Interfaces.Services;
using Ecommerce.Core.ViewModels;

namespace Ecommerce.Web.Controllers
{
    /// <summary>
    /// Session cart. The three mutations are POST + anti-forgery and return JSON for the
    /// AJAX flow (site.js refreshes the header MiniCart with /Cart/MiniCart afterwards).
    /// </summary>
    public class CartController : Controller
    {
        private readonly ICartService _cart;

        public CartController(ICartService cart)
        {
            _cart = cart;
        }

        /// <summary>Full cart page.</summary>
        public ActionResult Index()
        {
            return View(_cart.GetCart());
        }

        /// <summary>
        /// MiniCart partial for the header — called by _Layout on first render and by
        /// jQuery .load() after every cart change. Returns a partial (not a child action)
        /// so it is directly addressable by AJAX.
        /// </summary>
        public ActionResult MiniCart()
        {
            return PartialView("_MiniCart", _cart.GetMiniCart());
        }

        /// <summary>AJAX add-to-cart (spec: POST + JSON result).</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(int productId, int? variantId, int quantity = 1)
        {
            try
            {
                CartViewModel cart = _cart.AddItem(productId, variantId, quantity);
                CartLineViewModel last = cart.Lines.Count > 0 ? cart.Lines[cart.Lines.Count - 1] : null;

                return Json(new
                {
                    success = true,
                    itemCount = cart.ItemCount,
                    subTotal = cart.SubTotal,
                    productName = last != null ? last.ProductName : null
                });
            }
            catch (InvalidOperationException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>AJAX quantity update (quantity &lt;= 0 removes the line).</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(int productId, int? variantId, int quantity)
        {
            try
            {
                CartViewModel cart = _cart.UpdateQuantity(productId, variantId, quantity);
                return Json(new { success = true, itemCount = cart.ItemCount, subTotal = cart.SubTotal });
            }
            catch (InvalidOperationException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>AJAX remove line.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Remove(int productId, int? variantId)
        {
            try
            {
                CartViewModel cart = _cart.RemoveItem(productId, variantId);
                return Json(new { success = true, itemCount = cart.ItemCount, subTotal = cart.SubTotal });
            }
            catch (InvalidOperationException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
