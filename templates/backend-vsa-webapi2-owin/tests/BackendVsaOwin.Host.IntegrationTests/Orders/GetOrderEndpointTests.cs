using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using BackendVsaOwin.Host.IntegrationTests.Support;
using BackendVsaOwin.Modules.Orders.Features.GetOrder;
using BackendVsaOwin.BuildingBlocks.WebApi.Errors;
using Microsoft.Owin.Testing;
using Xunit;

namespace BackendVsaOwin.Host.IntegrationTests.Orders;

public sealed class GetOrderEndpointTests
{
    [Fact]
    public async Task Get_returns_the_created_order()
    {
        using var server = TestServer.Create<Startup>();
        using var client = TestServerFactory.CreateAuthenticatedClient(server);
        var created = await OrderEndpointTestHelper.CreateOrderAsync(
            server,
            "Ada Lovelace",
            42.50m);

        using var response = await client.GetAsync(
            $"api/orders/{created.Id}",
            TestContext.Current.CancellationToken);
        var order = await response.Content.ReadAsAsync<GetOrderResponse>(
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(created.Id, order.Id);
        Assert.Equal(created.CustomerId, order.CustomerId);
        Assert.Equal("Ada Lovelace", order.CustomerName);
        Assert.Equal(42.50m, order.TotalAmount);
    }

    [Fact]
    public async Task Get_returns_not_found_for_an_unknown_order()
    {
        using var server = TestServer.Create<Startup>();
        using var client = TestServerFactory.CreateAuthenticatedClient(server);
        var id = Guid.Parse("898001f1-8a22-45f9-82b0-bfa40520eb4e");

        using var response = await client.GetAsync(
            $"api/orders/{id}",
            TestContext.Current.CancellationToken);
        var problem = await ProblemDetailsAssert.ReadAsync<ProblemDetailsResponse>(
            response,
            "urn:backend-vsa-owin:problem:order-not-found");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Contains(id.ToString(), problem.Detail);
    }
}
