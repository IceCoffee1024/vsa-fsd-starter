using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using BackendVsaOwin.Host;
using BackendVsaOwin.Host.Composition;
using BackendVsaOwin.Host.Persistence;
using Microsoft.Owin.Testing;

namespace BackendVsaOwin.Host.IntegrationTests.Support;

internal static class TestServerFactory
{
    public static TestServer Create()
    {
        return TestServer.Create<DatabaseTestStartup>();
    }

    public static TestServer Create(string databasePath)
    {
        return TestServer.Create(
            app => Startup.Configure(
                app,
                ModuleCatalog.ControllerAssemblies,
                DatabaseOptions.Create(databasePath, pooling: false)));
    }

    public static HttpClient CreateAuthenticatedClient(TestServer server)
    {
        if (server is null)
        {
            throw new ArgumentNullException(nameof(server));
        }

        var client = server.HttpClient;
        var credentials = Convert.ToBase64String(
            Encoding.UTF8.GetBytes("test-user:test-password"));
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Basic", credentials);
        return client;
    }
}
