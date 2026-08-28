using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using BackendVsaOwin.Host.Authentication;
using BackendVsaOwin.Host.Authentication.DataProtection;
using BackendVsaOwin.Host.Authentication.RefreshTokens;
using BackendVsaOwin.Host.Composition;
using BackendVsaOwin.Host.OpenApi;
using BackendVsaOwin.Host.Persistence;
using BackendVsaOwin.Host.WebApi;
using BackendVsaOwin.BuildingBlocks.Persistence.Sqlite;
using HostApplicationIdentity = BackendVsaOwin.Host.Composition.ApplicationIdentity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Owin.BuilderProperties;
using Microsoft.Owin.Security.DataProtection;
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
        services.AddSingleton<IRefreshTokenStore, SqliteRefreshTokenStore>();
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
            app.SetDataProtectionProvider(
                AesDataProtectionProvider.FromConfiguration(
                    serviceProvider.GetRequiredService<ILogger<AesDataProtectionProvider>>()));
            SqliteDatabaseInitializer.Initialize(connectionFactory);
            SqliteMigrationRunner.Migrate(
                connectionFactory,
                MigrationCatalog.Assemblies,
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
            OAuthOptionsFactory.CreateAuthorizationServer(
                credentialValidator,
                serviceProvider.GetRequiredService<IRefreshTokenStore>()));
        app.UseOAuthBearerAuthentication(OAuthOptionsFactory.CreateBearer());
        app.UseBasicAuthentication(
            new BasicAuthenticationOptions(HostApplicationIdentity.BasicRealm),
            credentialValidator);
        app.UseWebApi(httpConfiguration);
        httpConfiguration.EnsureInitialized();
    }

    private static void DisposeOnShutdown(IAppBuilder app, IDisposable disposable)
    {
        var cancellationToken = new AppProperties(app.Properties).OnAppDisposing;
        if (cancellationToken.CanBeCanceled)
        {
            cancellationToken.Register(disposable.Dispose);
        }
    }
}
