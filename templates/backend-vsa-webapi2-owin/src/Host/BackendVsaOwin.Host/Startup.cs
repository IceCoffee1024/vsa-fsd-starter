using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using BackendVsaOwin.Host.Authentication;
using BackendVsaOwin.Host.Composition;
using BackendVsaOwin.Host.OpenApi;
using BackendVsaOwin.Host.Persistence;
using BackendVsaOwin.Host.WebApi;
using BackendVsaOwin.BuildingBlocks.Persistence.Sqlite;
using HostApplicationIdentity = BackendVsaOwin.Host.Composition.ApplicationIdentity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Owin;

namespace BackendVsaOwin.Host;

public sealed class Startup
{
    public void Configuration(IAppBuilder app)
    {
        Configure(
            app,
            ModuleCatalog.ControllerAssemblies,
            DatabaseOptions.FromConfiguration());
    }

    internal static void Configure(
        IAppBuilder app,
        IEnumerable<Assembly> controllerAssemblies,
        DatabaseOptions databaseOptions)
    {
        var connectionFactory = new SqliteConnectionFactory(
            databaseOptions.DatabasePath,
            databaseOptions.Pooling);
        var services = new ServiceCollection();
        services.AddSingleton(connectionFactory);
        ModuleCatalog.RegisterServices(services);
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

        try
        {
            SqliteDatabaseInitializer.Initialize(connectionFactory);
            SqliteMigrationRunner.Migrate(
                connectionFactory,
                ModuleCatalog.MigrationAssemblies,
                serviceProvider.GetRequiredService<ILoggerFactory>());
        }
        catch
        {
            serviceProvider.Dispose();
            throw;
        }

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
