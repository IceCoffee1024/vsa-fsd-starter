using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using BackendVsaOwin.Host.IntegrationTests.Support;
using BackendVsaOwin.BuildingBlocks.WebApi.Errors;
using BackendVsaOwin.BuildingBlocks.WebApi.Tracing;
using Microsoft.Owin.Testing;
using Xunit;

namespace BackendVsaOwin.Host.IntegrationTests.Pipeline;

public sealed class ProblemDetailsPipelineTests
{
    private const string ParentTraceId =
        "4bf92f3577b34da6a3ce929d0e0e4736";

    [Fact]
    public async Task Unhandled_exception_returns_a_safe_problem_response()
    {
        using var server = TestServer.Create<TestStartup>();
        using var client = TestServerFactory.CreateAuthenticatedClient(server);

        using var response = await client.GetAsync(
            "__integration-tests/fault",
            TestContext.Current.CancellationToken);
        var problem = await ProblemDetailsAssert.ReadAsync<ProblemDetailsResponse>(
            response,
            "about:blank");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("Internal Server Error", problem.Title);
        Assert.DoesNotContain("Sensitive diagnostic detail", body);
        Assert.DoesNotContain("InvalidOperationException", body);
    }

    [Fact]
    public async Task Production_startup_does_not_discover_test_controllers()
    {
        using var server = TestServer.Create<Startup>();
        using var client = TestServerFactory.CreateAuthenticatedClient(server);

        using var response = await client.GetAsync(
            "__integration-tests/fault",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Request_continues_a_valid_w3c_trace_context()
    {
        using var server = TestServer.Create<Startup>();
        using var client = TestServerFactory.CreateAuthenticatedClient(server);
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            "api/orders");
        request.Headers.TryAddWithoutValidation(
            "traceparent",
            $"00-{ParentTraceId}-00f067aa0ba902b7-01");

        using var response = await client.SendAsync(
            request,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(
            ParentTraceId,
            Assert.Single(response.Headers.GetValues(
                RequestTraceHandler.TraceIdHeaderName)));
    }
}
