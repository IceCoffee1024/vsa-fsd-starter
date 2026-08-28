using System;
using System.Collections.Generic;
using System.IO;
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
        using var server = TestServerFactory.Create();
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
        Assert.False(string.IsNullOrWhiteSpace(token.RefreshToken));
        Assert.InRange(token.ExpiresIn, 3590, 3600);
    }

    [Fact]
    public async Task Refresh_token_rotates_and_authenticates_the_api()
    {
        using var server = TestServerFactory.Create();
        using var client = server.HttpClient;
        var initial = await IssueTokenAsync(client);

        var refreshed = await RefreshAsync(
            client,
            initial.RefreshToken
                ?? throw new InvalidOperationException("No refresh token was issued."));

        Assert.False(string.IsNullOrWhiteSpace(refreshed.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(refreshed.RefreshToken));
        Assert.NotEqual(initial.RefreshToken, refreshed.RefreshToken);
        Assert.InRange(refreshed.ExpiresIn, 3590, 3600);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", refreshed.AccessToken);
        using var response = await client.GetAsync(
            "api/orders",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Replaying_a_refresh_token_revokes_its_token_family()
    {
        using var server = TestServerFactory.Create();
        using var client = server.HttpClient;
        var initial = await IssueTokenAsync(client);
        var initialRefreshToken = initial.RefreshToken
            ?? throw new InvalidOperationException("No refresh token was issued.");
        var rotated = await RefreshAsync(client, initialRefreshToken);

        using var replayContent = CreateRefreshTokenRequest(initialRefreshToken);
        using var replayResponse = await client.PostAsync(
            "oauth/token",
            replayContent,
            TestContext.Current.CancellationToken);
        using var rotatedContent = CreateRefreshTokenRequest(
            rotated.RefreshToken
                ?? throw new InvalidOperationException("No rotated refresh token was issued."));
        using var rotatedResponse = await client.PostAsync(
            "oauth/token",
            rotatedContent,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, replayResponse.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, rotatedResponse.StatusCode);
    }

    [Fact]
    public async Task Refresh_token_is_bound_to_the_original_public_client_id()
    {
        using var server = TestServerFactory.Create();
        using var client = server.HttpClient;
        using var initialContent = CreateTokenRequest(
            "test-password",
            "original-client");
        using var initialResponse = await client.PostAsync(
            "oauth/token",
            initialContent,
            TestContext.Current.CancellationToken);
        var initial = await initialResponse.Content.ReadAsAsync<OAuthTokenResponse>(
            TestContext.Current.CancellationToken);
        using var refreshContent = CreateRefreshTokenRequest(
            initial.RefreshToken
                ?? throw new InvalidOperationException("No refresh token was issued."),
            "different-client");

        using var response = await client.PostAsync(
            "oauth/token",
            refreshContent,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, initialResponse.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_token_survives_a_host_restart()
    {
        var directory = Path.Combine(
            Path.GetTempPath(),
            "BackendVsaOwin.Tests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var databasePath = Path.Combine(directory, "refresh-token-restart.db");

        try
        {
            OAuthTokenResponse initial;
            using (var firstServer = TestServerFactory.Create(databasePath))
            {
                initial = await IssueTokenAsync(firstServer.HttpClient);
            }

            using var secondServer = TestServerFactory.Create(databasePath);
            var refreshed = await RefreshAsync(
                secondServer.HttpClient,
                initial.RefreshToken
                    ?? throw new InvalidOperationException("No refresh token was issued."));

            Assert.False(string.IsNullOrWhiteSpace(refreshed.AccessToken));
            Assert.False(string.IsNullOrWhiteSpace(refreshed.RefreshToken));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task Bearer_token_authenticates_the_same_api_as_basic()
    {
        using var server = TestServerFactory.Create();
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
        using var server = TestServerFactory.Create();
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
        using var server = TestServerFactory.Create();
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
        using var server = TestServerFactory.Create();
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
        using var server = TestServerFactory.Create();
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
        using var server = TestServerFactory.Create();
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
        var token = await IssueTokenAsync(client);
        return token.AccessToken
            ?? throw new InvalidOperationException(
                "The token endpoint returned no access token.");
    }

    private static async Task<OAuthTokenResponse> IssueTokenAsync(HttpClient client)
    {
        using var content = CreateTokenRequest("test-password");
        using var response = await client.PostAsync(
            "oauth/token",
            content,
            TestContext.Current.CancellationToken);
        var token = await response.Content.ReadAsAsync<OAuthTokenResponse>(
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return token;
    }

    private static async Task<OAuthTokenResponse> RefreshAsync(
        HttpClient client,
        string refreshToken)
    {
        using var content = CreateRefreshTokenRequest(refreshToken);
        using var response = await client.PostAsync(
            "oauth/token",
            content,
            TestContext.Current.CancellationToken);
        var token = await response.Content.ReadAsAsync<OAuthTokenResponse>(
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return token;
    }

    private static FormUrlEncodedContent CreateTokenRequest(
        string password,
        string? clientId = null)
    {
        var parameters = new List<KeyValuePair<string, string>>
        {
            new(
                OAuthParameters.GrantType,
                OAuthParameters.PasswordGrantType),
            new(
                OAuthParameters.Username,
                "test-user"),
            new(
                OAuthParameters.Password,
                password),
        };
        if (clientId is not null)
        {
            parameters.Add(new KeyValuePair<string, string>(
                OAuthParameters.ClientId,
                clientId));
        }

        return new FormUrlEncodedContent(parameters);
    }

    private static FormUrlEncodedContent CreateRefreshTokenRequest(
        string refreshToken,
        string? clientId = null)
    {
        var parameters = new List<KeyValuePair<string, string>>
        {
            new(
                OAuthParameters.GrantType,
                OAuthParameters.RefreshTokenGrantType),
            new(
                OAuthParameters.RefreshToken,
                refreshToken),
        };
        if (clientId is not null)
        {
            parameters.Add(new KeyValuePair<string, string>(
                OAuthParameters.ClientId,
                clientId));
        }

        return new FormUrlEncodedContent(parameters);
    }

    private sealed class OAuthTokenResponse
    {
        [JsonProperty(OAuthParameters.AccessToken)]
        public string? AccessToken { get; set; }

        [JsonProperty(OAuthParameters.RefreshToken)]
        public string? RefreshToken { get; set; }

        [JsonProperty("token_type")]
        public string? TokenType { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }
    }
}
