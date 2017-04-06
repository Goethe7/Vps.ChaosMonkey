using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Vps.ChaosMonkeyMonitor.Startup))]
namespace Vps.ChaosMonkeyMonitor
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
