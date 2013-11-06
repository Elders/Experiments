using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(WEBSecurity.Startup))]
namespace WEBSecurity
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
