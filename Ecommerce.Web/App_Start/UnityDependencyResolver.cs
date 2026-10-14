using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Unity;

namespace Ecommerce.Web.App_Start
{
    /// <summary>
    /// Minimal MVC5 dependency resolver over the Unity 5.11 container
    /// (replaces the abandoned Unity.Mvc5 package). The container provider returns
    /// the per-request child container (UnityConfig.CurrentContainer) during a
    /// request and the root container otherwise.
    /// </summary>
    public class UnityDependencyResolver : IDependencyResolver
    {
        private readonly Func<IUnityContainer> _containerProvider;

        public UnityDependencyResolver(Func<IUnityContainer> containerProvider)
        {
            if (containerProvider == null) throw new ArgumentNullException("containerProvider");
            _containerProvider = containerProvider;
        }

        public object GetService(Type serviceType)
        {
            if (serviceType == null) return null;

            try
            {
                IUnityContainer container = _containerProvider();
                if (container.IsRegistered(serviceType))
                {
                    return container.Resolve(serviceType);
                }

                // Concrete classes (e.g. a controller not explicitly registered) can still be
                // built from registered dependencies; interfaces fall through to null and MVC
                // uses its own defaults.
                if (serviceType.IsClass && !serviceType.IsAbstract)
                {
                    return container.Resolve(serviceType);
                }
            }
            catch (Exception ex)
            {
                // A registered/concrete type failed to build: surface the real Unity failure.
                // (Returning null here would hide the cause — MVC would fall back to
                // Activator.CreateInstance and report a misleading "no parameterless
                // constructor" error instead.)
                throw new InvalidOperationException(
                    "Unity failed to resolve " + serviceType.FullName, ex);
            }

            return null;
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            try
            {
                return _containerProvider().ResolveAll(serviceType).ToList();
            }
            catch (Exception)
            {
                return Enumerable.Empty<object>();
            }
        }
    }
}
