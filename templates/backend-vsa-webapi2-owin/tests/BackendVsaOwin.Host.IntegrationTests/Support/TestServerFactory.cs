using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Owin.Testing;

namespace BackendVsaOwin.Host.IntegrationTests.Support;

internal static class TestServerFactory
{
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
