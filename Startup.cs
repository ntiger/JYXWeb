using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(JYXWeb.Startup))]
namespace JYXWeb
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
