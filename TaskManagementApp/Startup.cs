using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(TaskManagementApp.Startup))]
namespace TaskManagementApp
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
