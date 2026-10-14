using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Ecommerce.Web.Startup))]

namespace Ecommerce.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
