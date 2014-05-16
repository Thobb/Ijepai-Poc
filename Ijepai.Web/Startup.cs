using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Ijepai.Web.Startup))]
namespace Ijepai.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
