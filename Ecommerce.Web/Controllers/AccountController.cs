using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Ecommerce.Core.Interfaces.Services;
using Ecommerce.Core.ViewModels;
using Ecommerce.Web.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;

namespace Ecommerce.Web.Controllers
{
    /// <summary>
    /// Registration / login / logout / order history. Credential handling is ASP.NET
    /// Identity 2.0 via OWIN (per-request managers, not Unity); the only Unity dependency
    /// is IAccountService (Customer profile + order history).
    /// </summary>
    public class AccountController : Controller
    {
        private readonly IAccountService _account;

        public AccountController(IAccountService account)
        {
            _account = account;
        }

        // Per-request OWIN managers (registered in Startup.Auth — not Unity).
        private ApplicationSignInManager SignInManager
        {
            get { return HttpContext.GetOwinContext().Get<ApplicationSignInManager>(); }
        }

        private ApplicationUserManager UserManager
        {
            get { return HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>(); }
        }

        private IAuthenticationManager AuthenticationManager
        {
            get { return HttpContext.GetOwinContext().Authentication; }
        }

        // ------------------------------------------------------------------ login

        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            PasswordSignInResult result = await SignInManager.PasswordSignInAsync(
                model.Email, model.Password, model.RememberMe, shouldLockout: true);

            switch (result)
            {
                case PasswordSignInResult.Success:
                    return RedirectToLocal(returnUrl);
                case PasswordSignInResult.LockedOut:
                    ModelState.AddModelError(string.Empty,
                        "This account has been locked out after too many failed attempts. Please try again later.");
                    return View(model);
                default:
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View(model);
            }
        }

        // ------------------------------------------------------------- register

        public ActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
            IdentityResult result = await UserManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Customer profile row (dbo.Customers) linked to the Identity user id.
                _account.EnsureCustomerProfile(user.Id, user.Email, model.FullName);

                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                // The SQL script seeds the "Customer" role — assign it when it exists.
                try
                {
                    if (!await UserManager.IsInRoleAsync(user.Id, "Customer"))
                    {
                        await UserManager.AddToRoleAsync(user.Id, "Customer");
                    }
                }
                catch (InvalidOperationException)
                {
                    // Role not seeded yet (SQL script not executed) — registration still succeeds.
                }

                return RedirectToAction("Index", "Home");
            }

            AddErrors(result);
            return View(model);
        }

        // ---------------------------------------------------------------- logout

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            // Identity 2.0.1: IAuthenticationManager.SignOut is synchronous (SignOutAsync arrived later).
            AuthenticationManager.SignOut(
                DefaultAuthenticationTypes.ApplicationCookie,
                DefaultAuthenticationTypes.ExternalCookie);
            return RedirectToAction("Index", "Home");
        }

        // ----------------------------------------------------------- order history

        [Authorize]
        public ActionResult Orders()
        {
            string userId = User.Identity.GetUserId();
            OrderHistoryViewModel model = _account.GetOrderHistory(userId);
            if (string.IsNullOrEmpty(model.Email))
            {
                var identity = (System.Security.Claims.ClaimsIdentity)User.Identity;
                model.Email = identity.Name;
            }
            return View(model);
        }

        // ---------------------------------------------------------------- helpers

        private void AddErrors(IdentityResult result)
        {
            foreach (string error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }
    }
}
