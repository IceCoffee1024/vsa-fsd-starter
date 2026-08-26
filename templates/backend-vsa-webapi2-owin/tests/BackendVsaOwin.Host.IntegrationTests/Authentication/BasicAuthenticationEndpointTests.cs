using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using BackendVsaOwin.Host.Authentication;
using BackendVsaOwin.Host.IntegrationTests.Support;
using Microsoft.Owin.Testing;
using Xunit;

namespace BackendVsaOwin.Host.IntegrationTests.Authentication;

public sealed class BasicAuthenticationEndpointTests
{
    [Fact]
    public async Task Valid_credentials_establish_a_basic_identity_for_web_api()
    {
        using var server = TestServer.Create<TestStartup>();
        using var client = TestServerFactory.CreateAuthenticatedClient(server);

        using var response = await client.GetAsync(
            "__integration-tests/authentication",
            TestContext.Current.CancellationToken);
        var identity = await response.Content.ReadAsAsync<AuthenticationProbeResponse>(
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("test-user", identity.Name);
        Assert.Equal(BasicAuthenticationOptions.Scheme, identity.AuthenticationType);
        Assert.True(identity.IsAuthenticated);
    }

    [Fact]
    public async Task Api_request_without_credentials_returns_basic_challenge()
    {
        using var server = TestServerFactory.Create();
        using var client = server.HttpClient;

        using var response = await client.GetAsync(
            "api/orders",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(
            new[]
            {
                "Bearer realm=\"BackendVsaOwin\"",
                "Basic realm=\"BackendVsaOwin\"",
            },
            response.Headers.GetValues("WWW-Authenticate"));
    }

    [Fact]
    public async Task Api_request_with_wrong_credentials_returns_basic_challenge()
    {
        using var server = TestServerFactory.Create();
        using var client = server.HttpClient;
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Basic",
                Convert.ToBase64String(
                    Encoding.UTF8.GetBytes("test-user:wrong-password")));

        using var response = await client.GetAsync(
            "api/orders",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(
            "Basic realm=\"BackendVsaOwin\"",
            Assert.Single(response.Headers.GetValues("WWW-Authenticate")));
    }

    [Theory]
    [InlineData("swagger/v1/swagger.json")]
    [InlineData("swagger/index.html")]
    public async Task Swagger_remains_public_without_credentials(string path)
    {
        using var server = TestServerFactory.Create();
        using var client = server.HttpClient;

        using var response = await client.GetAsync(
            path,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private sealed class AuthenticationProbeResponse
    {
        public string? Name { get; set; }

        public string? AuthenticationType { get; set; }

        public bool IsAuthenticated { get; set; }
    }
}
