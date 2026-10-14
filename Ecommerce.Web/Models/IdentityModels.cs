using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;

namespace Ecommerce.Web.Models
{
    /// <summary>
    /// Profile data for application users — stored in AspNetUsers (created by the SQL script).
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType.
            ClaimsIdentity userIdentity = await manager.CreateIdentityAsync(
                this, DefaultAuthenticationTypes.ApplicationCookie);
            return userIdentity;
        }
    }

    /// <summary>
    /// ASP.NET Identity 2.0 context (code-first over the Identity tables created by
    /// database\01_Create_LegacyEcommerceDb.sql — same database as the EDMX context).
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("EcommerceIdentityDb", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        /// <summary>EF must never create/migrate these tables — the reviewed SQL script owns them.</summary>
        internal static void DisableAutomaticInitialization()
        {
            // Static call — must be fully qualified (DbContext also has an instance Database property).
            System.Data.Entity.Database.SetInitializer<ApplicationDbContext>(null);
        }
    }

    /// <summary>Configured in Application_Start via OWIN per-request factories (not Unity).</summary>
    public class ApplicationUserManager : UserManager<ApplicationUser>
    {
        public ApplicationUserManager(IUserStore<ApplicationUser> store)
            : base(store)
        {
        }

        public static ApplicationUserManager Create(
            IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context)
        {
            var manager = new ApplicationUserManager(
                new UserStore<ApplicationUser>(context.Get<ApplicationDbContext>()));

            manager.UserValidator = new UserValidator<ApplicationUser>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };

            // Password rules mirrored by RegisterViewModel annotations (min 6 chars).
            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 6,
                RequireNonLetterOrDigit = true,
                RequireDigit = true,
                RequireLowercase = true,
                RequireUppercase = true
            };

            manager.UserLockoutEnabledByDefault = true;
            manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
            manager.MaxFailedAccessAttemptsBeforeLockout = 5;

            manager.EmailService = new NoOpEmailService();
            manager.SmsService = new NoOpSmsService();

            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider = new DataProtectorTokenProvider<ApplicationUser>(
                    dataProtectionProvider.Create("ASP.NET Identity"));
            }

            return manager;
        }
    }

    /// <summary>Password sign-in outcome for the hand-rolled sign-in below.</summary>
    public enum PasswordSignInResult
    {
        Success,
        LockedOut,
        Failure
    }

    /// <summary>
    /// Minimal sign-in helper. Microsoft.AspNet.Identity.Owin 2.0.1 (the pinned legacy version)
    /// does not ship SignInManager&lt;,&gt; / SignInStatus (those arrived in Identity 2.1), so
    /// sign-in is implemented here directly over UserManager + the OWIN cookie middleware.
    /// </summary>
    public class ApplicationSignInManager : IDisposable
    {
        private readonly ApplicationUserManager _userManager;
        private readonly IAuthenticationManager _auth;

        public ApplicationSignInManager(ApplicationUserManager userManager, IAuthenticationManager authenticationManager)
        {
            _userManager = userManager;
            _auth = authenticationManager;
        }

        public static ApplicationSignInManager Create(
            IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context)
        {
            return new ApplicationSignInManager(
                context.GetUserManager<ApplicationUserManager>(), context.Authentication);
        }

        public async Task<PasswordSignInResult> PasswordSignInAsync(
            string email, string password, bool rememberMe, bool shouldLockout)
        {
            ApplicationUser user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return PasswordSignInResult.Failure;
            }

            if (await _userManager.IsLockedOutAsync(user.Id))
            {
                return PasswordSignInResult.LockedOut;
            }

            if (await _userManager.CheckPasswordAsync(user, password))
            {
                await _userManager.ResetAccessFailedCountAsync(user.Id);
                await SignInAsync(user, rememberMe, false);
                return PasswordSignInResult.Success;
            }

            if (shouldLockout)
            {
                await _userManager.AccessFailedAsync(user.Id);
                if (await _userManager.IsLockedOutAsync(user.Id))
                {
                    return PasswordSignInResult.LockedOut;
                }
            }

            return PasswordSignInResult.Failure;
        }

        public async Task SignInAsync(ApplicationUser user, bool isPersistent, bool rememberBrowser)
        {
            _auth.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            ClaimsIdentity identity = await user.GenerateUserIdentityAsync(_userManager);
            _auth.SignIn(new AuthenticationProperties { IsPersistent = isPersistent }, identity);
        }

        public void Dispose()
        {
        }
    }

    /// <summary>No SMTP / SMS in this demo — hooks stay wired so the extension points are visible.</summary>
    public class NoOpEmailService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            return Task.FromResult(0);
        }
    }

    public class NoOpSmsService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            return Task.FromResult(0);
        }
    }
}
