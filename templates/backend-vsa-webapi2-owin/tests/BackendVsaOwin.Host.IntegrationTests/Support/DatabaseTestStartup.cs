using BackendVsaOwin.Host;
using BackendVsaOwin.Host.Composition;
using Owin;

namespace BackendVsaOwin.Host.IntegrationTests.Support;

public sealed class DatabaseTestStartup
{
    public void Configuration(IAppBuilder app)
    {
        Startup.Configure(
            app,
            ModuleCatalog.ControllerAssemblies,
            TemporaryDatabase.CreateOptions(app));
    }
}
