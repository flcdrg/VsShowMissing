using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Tests.Startup))]
namespace Tests
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
