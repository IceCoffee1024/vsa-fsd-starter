using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using BackendVsaOwin.Host.Authentication;
using BackendVsaOwin.Host.Composition;
using BackendVsaOwin.Modules.Customers;
using BackendVsaOwin.Modules.Orders;
using BackendVsaOwin.Host.OpenApi;
using BackendVsaOwin.Host.WebApi;
using HostApplicationIdentity = BackendVsaOwin.Host.Composition.ApplicationIdentity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Owin;

namespace BackendVsaOwin.Host;

public sealed class Startup
{
    public void Configuration(IAppBuilder app)
    {
        Configure(app, ModuleCatalog.Assemblies);
    }

    internal static void Configure(
        IAppBuilder app,
        IEnumerable<Assembly> controllerAssemblies)
    {
        var services = new ServiceCollection();
        services.AddCustomersModule();
        services.AddOrdersModule();
        services.AddLogging(logging =>
        {
            logging.AddJsonConsole(options =>
            {
                options.TimestampFormat = "O";
                options.UseUtcTimestamp = true;
            });
        });

        var serviceProvider = services.BuildServiceProvider(
            new ServiceProviderOptions
            {
                ValidateOnBuild = true,
                ValidateScopes = true,
            });

        var httpConfiguration = WebApiConfig.Create(
            serviceProvider,
            controllerAssemblies);

        DisposeOnShutdown(app, httpConfiguration);
        DisposeOnShutdown(app, serviceProvider);

        app.UseConfiguredOpenApi(
            controllerAssemblies,
            httpConfiguration.Formatters.JsonFormatter.SerializerSettings);
        var credentialValidator = ConfiguredCredentialValidator.FromConfiguration();
        app.UseOAuthAuthorizationServer(
            OAuthOptionsFactory.CreateAuthorizationServer(credentialValidator));
        app.UseOAuthBearerAuthentication(OAuthOptionsFactory.CreateBearer());
        app.UseBasicAuthentication(
            new BasicAuthenticationOptions(HostApplicationIdentity.BasicRealm),
            credentialValidator);
        app.UseWebApi(httpConfiguration);
        httpConfiguration.EnsureInitialized();
    }

    private static void DisposeOnShutdown(IAppBuilder app, IDisposable disposable)
    {
        if (app.Properties.TryGetValue("host.OnAppDisposing", out var value)
            && value is CancellationToken cancellationToken)
        {
            cancellationToken.Register(disposable.Dispose);
        }
    }
}
