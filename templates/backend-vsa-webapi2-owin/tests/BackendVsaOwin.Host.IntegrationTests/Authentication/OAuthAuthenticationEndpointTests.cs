using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using BackendVsaOwin.Host.Authentication;
using BackendVsaOwin.Host.IntegrationTests.Support;
using Microsoft.Owin.Testing;
using Newtonsoft.Json;
using Xunit;

namespace BackendVsaOwin.Host.IntegrationTests.Authentication;

public sealed class OAuthAuthenticationEndpointTests
{
    [Fact]
    public async Task Valid_resource_owner_credentials_issue_a_bearer_token()
    {
        using var server = TestServer.Create<Startup>();
        using var client = server.HttpClient;
        using var content = CreateTokenRequest("test-password");

        using var response = await client.PostAsync(
            "oauth/token",
            content,
            TestContext.Current.CancellationToken);
        var token = await response.Content.ReadAsAsync<OAuthTokenResponse>(
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("bearer", token.TokenType);
        Assert.False(string.IsNullOrWhiteSpace(token.AccessToken));
    }

    [Fact]
    public async Task Bearer_token_authenticates_the_same_api_as_basic()
    {
        using var server = TestServer.Create<Startup>();
        using var client = server.HttpClient;
        var accessToken = await IssueAccessTokenAsync(client);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await client.GetAsync(
            "api/orders",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Query_string_bearer_token_authenticates_the_api()
    {
        using var server = TestServer.Create<Startup>();
        using var client = server.HttpClient;
        var accessToken = await IssueAccessTokenAsync(client);

        using var response = await client.GetAsync(
            $"api/orders?{OAuthParameters.AccessToken}="
                + Uri.EscapeDataString(accessToken),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Bearer_header_takes_precedence_over_query_string_token()
    {
        using var server = TestServer.Create<Startup>();
        using var client = server.HttpClient;
        var accessToken = await IssueAccessTokenAsync(client);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "invalid-token");

        using var response = await client.GetAsync(
            $"api/orders?{OAuthParameters.AccessToken}="
                + Uri.EscapeDataString(accessToken),
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(
            new[] { "Bearer realm=\"BackendVsaOwin\"" },
            response.Headers.GetValues("WWW-Authenticate"));
    }

    [Fact]
    public async Task Invalid_resource_owner_credentials_do_not_issue_a_token()
    {
        using var server = TestServer.Create<Startup>();
        using var client = server.HttpClient;
        using var content = CreateTokenRequest("wrong-password");

        using var response = await client.PostAsync(
            "oauth/token",
            content,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Invalid_bearer_token_returns_only_a_bearer_challenge()
    {
        using var server = TestServer.Create<Startup>();
        using var client = server.HttpClient;
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "invalid-token");

        using var response = await client.GetAsync(
            "api/orders",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(
            new[] { "Bearer realm=\"BackendVsaOwin\"" },
            response.Headers.GetValues("WWW-Authenticate"));
    }

    [Fact]
    public async Task Invalid_query_string_token_returns_only_a_bearer_challenge()
    {
        using var server = TestServer.Create<Startup>();
        using var client = server.HttpClient;

        using var response = await client.GetAsync(
            $"api/orders?{OAuthParameters.AccessToken}=invalid-token",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(
            new[] { "Bearer realm=\"BackendVsaOwin\"" },
            response.Headers.GetValues("WWW-Authenticate"));
    }

    private static async Task<string> IssueAccessTokenAsync(HttpClient client)
    {
        using var content = CreateTokenRequest("test-password");
        using var response = await client.PostAsync(
            "oauth/token",
            content,
            TestContext.Current.CancellationToken);
        var token = await response.Content.ReadAsAsync<OAuthTokenResponse>(
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return token.AccessToken
            ?? throw new InvalidOperationException(
                "The token endpoint returned no access token.");
    }

    private static FormUrlEncodedContent CreateTokenRequest(string password)
    {
        return new FormUrlEncodedContent(
            new[]
            {
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("username", "test-user"),
                new KeyValuePair<string, string>("password", password),
            });
    }

    private sealed class OAuthTokenResponse
    {
        [JsonProperty(OAuthParameters.AccessToken)]
        public string? AccessToken { get; set; }

        [JsonProperty("token_type")]
        public string? TokenType { get; set; }
    }
}
