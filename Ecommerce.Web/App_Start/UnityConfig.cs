using System;
using System.Web;
using System.Web.Mvc;
using Ecommerce.Core.Interfaces;
using Ecommerce.Core.Interfaces.Repositories;
using Ecommerce.Core.Interfaces.Services;
using Ecommerce.Data;
using Ecommerce.Services;
using Ecommerce.Web.Controllers;
using Ecommerce.Web.Infrastructure;
using Unity;
using Unity.Lifetime;

namespace Ecommerce.Web.App_Start
{
    /// <summary>
    /// Unity 5.11.10 composition root. Per-request scoping is done with a child container
    /// created in Application_BeginRequest (Global.asax) and disposed in Application_EndRequest:
    /// resolving EcommerceDbContext with a HierarchicalLifetimeManager yields ONE context
    /// instance per request, shared by all repositories + the unit of work.
    /// Controllers -> services -> repositories only; no service ever new's a dependency.
    /// </summary>
    public static class UnityConfig
    {
        private const string PerRequestContainerKey = "__UnityPerRequestContainer";

        private static readonly Lazy<UnityContainer> LazyContainer =
            new Lazy<UnityContainer>(() =>
            {
                var container = new UnityContainer();
                RegisterTypes(container);
                return container;
            });

        /// <summary>The root container (application lifetime).</summary>
        public static IUnityContainer Container
        {
            get { return LazyContainer.Value; }
        }

        /// <summary>The child container for the current HTTP request (falls back to the root).</summary>
        public static IUnityContainer CurrentContainer
        {
            get
            {
                var items = HttpContext.Current != null ? HttpContext.Current.Items : null;
                var child = items != null ? items[PerRequestContainerKey] as IUnityContainer : null;
                return child ?? Container;
            }
        }

        public static void RegisterComponents()
        {
            DependencyResolver.SetResolver(new UnityDependencyResolver(() => CurrentContainer));
        }

        /// <summary>Called from Global.asax Application_BeginRequest — one child container per request.</summary>
        public static void BeginRequest()
        {
            HttpContext httpContext = HttpContext.Current;
            if (httpContext != null)
            {
                httpContext.Items[PerRequestContainerKey] = Container.CreateChildContainer();
            }
        }

        /// <summary>
        /// Called from Global.asax Application_EndRequest — disposing the child container also
        /// disposes the per-request EcommerceDbContext it created (tracked by Unity).
        /// </summary>
        public static void EndRequest()
        {
            HttpContext httpContext = HttpContext.Current;
            if (httpContext == null) return;

            var child = httpContext.Items[PerRequestContainerKey] as IDisposable;
            httpContext.Items.Remove(PerRequestContainerKey);
            if (child != null)
            {
                child.Dispose();
            }
        }

        public static void RegisterTypes(IUnityContainer container)
        {
            // Hierarchical: ONE EcommerceDbContext per request (cached in the child container,
            // disposed with it) — repositories and unit of work share that single instance.
            container.RegisterType<EcommerceDbContext>(new HierarchicalLifetimeManager());

            // ---- Data layer (transient; all receive the shared per-request context) ----
            container.RegisterType<IUnitOfWork, EfUnitOfWork>();
            container.RegisterType<IProductRepository, ProductRepository>();
            container.RegisterType<ICategoryRepository, CategoryRepository>();
            container.RegisterType<ICartRepository, CartRepository>();
            container.RegisterType<IOrderRepository, OrderRepository>();
            container.RegisterType<ICustomerRepository, CustomerRepository>();

            // ---- Session state abstraction (implemented in this project) ---------------
            container.RegisterType<ISessionCartStore, HttpSessionCartStore>();

            // ---- Services --------------------------------------------------------------
            container.RegisterType<ICatalogService, CatalogService>();
            container.RegisterType<ICartService, CartService>();
            container.RegisterType<ICheckoutService, CheckoutService>();
            container.RegisterType<IAccountService, AccountService>();
            container.RegisterType<IPricingCalculator, PricingCalculator>();

            // ---- Controllers (transient — one instance per request) --------------------
            container.RegisterType<HomeController>();
            container.RegisterType<ProductController>();
            container.RegisterType<CartController>();
            container.RegisterType<CheckoutController>();
            container.RegisterType<AccountController>();
        }
    }
}
