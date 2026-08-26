using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using BackendVsaOwin.Host.IntegrationTests.Support;
using BackendVsaOwin.BuildingBlocks.WebApi.Errors;
using Microsoft.Owin.Testing;
using Xunit;

namespace BackendVsaOwin.Host.IntegrationTests.Orders;

public sealed class DeleteOrderEndpointTests
{
    [Fact]
    public async Task Delete_removes_the_order_and_then_returns_not_found()
    {
        using var server = TestServer.Create<Startup>();
        using var client = TestServerFactory.CreateAuthenticatedClient(server);
        var created = await OrderEndpointTestHelper.CreateOrderAsync(
            server,
            "Ada Lovelace",
            42.50m);

        using var deleteResponse = await client.DeleteAsync(
            $"api/orders/{created.Id}",
            TestContext.Current.CancellationToken);
        using var getResponse = await client.GetAsync(
            $"api/orders/{created.Id}",
            TestContext.Current.CancellationToken);
        var problem = await ProblemDetailsAssert.ReadAsync<ProblemDetailsResponse>(
            getResponse,
            "urn:backend-vsa-owin:problem:order-not-found");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        Assert.Contains(created.Id.ToString(), problem.Detail);
    }

    [Fact]
    public async Task Delete_returns_not_found_for_an_unknown_order()
    {
        using var server = TestServer.Create<Startup>();
        using var client = TestServerFactory.CreateAuthenticatedClient(server);
        var id = Guid.Parse("e1f1eadd-5983-42f8-9480-2459c90ad762");

        using var response = await client.DeleteAsync(
            $"api/orders/{id}",
            TestContext.Current.CancellationToken);
        var problem = await ProblemDetailsAssert.ReadAsync<ProblemDetailsResponse>(
            response,
            "urn:backend-vsa-owin:problem:order-not-found");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Contains(id.ToString(), problem.Detail);
    }
}
