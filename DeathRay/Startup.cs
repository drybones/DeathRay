using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(DeathRay.Startup))]
namespace DeathRay
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
