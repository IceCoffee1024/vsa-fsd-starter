using System.Linq;
using BackendVsaOwin.Host;
using BackendVsaOwin.Host.Composition;
using Owin;

namespace BackendVsaOwin.Host.IntegrationTests.Support;

/// <summary>
/// Adds the test assembly to the controller whitelist without changing production startup.
/// </summary>
public sealed class TestStartup
{
    public void Configuration(IAppBuilder app)
    {
        Startup.Configure(
            app,
            ModuleCatalog.Assemblies
                .Concat(new[] { typeof(FaultingController).Assembly }));
    }
}
