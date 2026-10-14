using System;
using System.Web.Mvc;
using Ecommerce.Core.Interfaces.Services;
using Ecommerce.Core.ViewModels;
using Microsoft.AspNet.Identity;

namespace Ecommerce.Web.Controllers
{
    /// <summary>
    /// Three-step checkout (Address -> Shipping -> Payment) under [Authorize].
    /// The wizard state lives in Session (CheckoutAddress / CheckoutShippingMethod);
    /// the confirmation is passed through TempData so no extra service method is needed.
    /// </summary>
    [Authorize]
    public class CheckoutController : Controller
    {
        private const string AddressKey = "CheckoutAddress";
        private const string ShippingKey = "CheckoutShippingMethod";
        private const string ConfirmationKey = "OrderConfirmation";

        private readonly ICheckoutService _checkout;

        public CheckoutController(ICheckoutService checkout)
        {
            _checkout = checkout;
        }

        // ---------------------------------------------------------------- step 1

        public ActionResult Address()
        {
            CheckoutAddressViewModel model = Session[AddressKey] as CheckoutAddressViewModel;
            return View(model ?? new CheckoutAddressViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Address(CheckoutAddressViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            Session[AddressKey] = model;
            return RedirectToAction("Shipping");
        }

        // ---------------------------------------------------------------- step 2

        public ActionResult Shipping()
        {
            if (Session[AddressKey] == null)
            {
                return RedirectToAction("Address");
            }

            try
            {
                string selected = Session[ShippingKey] as string;
                return View(_checkout.GetShippingStep(selected));
            }
            catch (InvalidOperationException)
            {
                // Cart emptied in another tab / session lost.
                return RedirectToAction("Index", "Cart");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Shipping(CheckoutShippingViewModel model)
        {
            if (Session[AddressKey] == null)
            {
                return RedirectToAction("Address");
            }

            if (string.IsNullOrWhiteSpace(model.ShippingMethod))
            {
                ModelState.AddModelError("ShippingMethod", "Please choose a shipping method.");
            }

            if (!ModelState.IsValid)
            {
                try
                {
                    CheckoutShippingViewModel vm = _checkout.GetShippingStep(model.ShippingMethod);
                    vm.ShippingMethod = model.ShippingMethod;
                    return View(vm);
                }
                catch (InvalidOperationException)
                {
                    return RedirectToAction("Index", "Cart");
                }
            }

            Session[ShippingKey] = model.ShippingMethod;
            return RedirectToAction("Payment");
        }

        // ---------------------------------------------------------------- step 3

        public ActionResult Payment()
        {
            if (Session[AddressKey] == null) return RedirectToAction("Address");
            if (Session[ShippingKey] == null) return RedirectToAction("Shipping");

            return View(new CheckoutPaymentViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Payment(CheckoutPaymentViewModel model)
        {
            if (Session[AddressKey] == null) return RedirectToAction("Address");
            if (Session[ShippingKey] == null) return RedirectToAction("Shipping");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var address = (CheckoutAddressViewModel)Session[AddressKey];
            var shipping = new CheckoutShippingViewModel { ShippingMethod = Session[ShippingKey] as string };

            try
            {
                OrderConfirmationViewModel confirmation = _checkout.PlaceOrder(
                    address, shipping, model, User.Identity.GetUserId());

                Session.Remove(AddressKey);
                Session.Remove(ShippingKey);

                TempData[ConfirmationKey] = confirmation;
                return RedirectToAction("Confirmation");
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        // ---------------------------------------------------------------- result

        public ActionResult Confirmation()
        {
            var model = TempData[ConfirmationKey] as OrderConfirmationViewModel;
            if (model == null)
            {
                return HttpNotFound();
            }
            return View(model);
        }

        /// <summary>Order summary box rendered on Shipping/Payment steps (VM-only view).</summary>
        [ChildActionOnly]
        public ActionResult OrderSummary()
        {
            try
            {
                string selected = Session[ShippingKey] as string;
                return PartialView("_OrderSummary", _checkout.GetShippingStep(selected));
            }
            catch (InvalidOperationException)
            {
                return PartialView("_OrderSummary", null);
            }
        }
    }
}
