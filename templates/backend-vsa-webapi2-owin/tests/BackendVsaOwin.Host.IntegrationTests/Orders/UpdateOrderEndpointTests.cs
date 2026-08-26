using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using BackendVsaOwin.Host.IntegrationTests.Support;
using BackendVsaOwin.Modules.Orders.Features.GetOrder;
using BackendVsaOwin.Modules.Orders.Features.UpdateOrder;
using BackendVsaOwin.BuildingBlocks.WebApi.Errors;
using Microsoft.Owin.Testing;
using Xunit;

namespace BackendVsaOwin.Host.IntegrationTests.Orders;

public sealed class UpdateOrderEndpointTests
{
    [Fact]
    public async Task Put_updates_the_existing_order()
    {
        using var server = TestServerFactory.Create();
        using var client = TestServerFactory.CreateAuthenticatedClient(server);
        var created = await OrderEndpointTestHelper.CreateOrderAsync(
            server,
            "Ada Lovelace",
            42.50m);
        var request = new UpdateOrderRequest
        {
            TotalAmount = 99.95m,
        };

        using var updateResponse = await client.PutAsJsonAsync(
            $"api/orders/{created.Id}",
            request);
        var updated = await updateResponse.Content.ReadAsAsync<UpdateOrderResponse>(
            TestContext.Current.CancellationToken);

        using var getResponse = await client.GetAsync(
            $"api/orders/{created.Id}",
            TestContext.Current.CancellationToken);
        var persisted = await getResponse.Content.ReadAsAsync<GetOrderResponse>(
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        Assert.Equal(created.CustomerId, updated.CustomerId);
        Assert.Equal("Ada Lovelace", updated.CustomerName);
        Assert.Equal(99.95m, updated.TotalAmount);
        Assert.Equal(created.CustomerId, persisted.CustomerId);
        Assert.Equal("Ada Lovelace", persisted.CustomerName);
        Assert.Equal(99.95m, persisted.TotalAmount);
    }

    [Fact]
    public async Task Put_rejects_invalid_input()
    {
        using var server = TestServerFactory.Create();
        using var client = TestServerFactory.CreateAuthenticatedClient(server);
        var created = await OrderEndpointTestHelper.CreateOrderAsync(
            server,
            "Ada Lovelace",
            42.50m);
        var request = new UpdateOrderRequest
        {
            TotalAmount = 0,
        };

        using var response = await client.PutAsJsonAsync(
            $"api/orders/{created.Id}",
            request);
        var problem = await ProblemDetailsAssert
            .ReadAsync<ValidationProblemDetailsResponse>(
                response,
                "urn:backend-vsa-owin:problem:validation-failed");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("totalAmount", problem.Errors.Keys);
    }

    [Fact]
    public async Task Put_returns_not_found_for_an_unknown_order()
    {
        using var server = TestServerFactory.Create();
        using var client = TestServerFactory.CreateAuthenticatedClient(server);
        var id = Guid.Parse("e1f1eadd-5983-42f8-9480-2459c90ad762");
        var request = new UpdateOrderRequest
        {
            TotalAmount = 99.95m,
        };

        using var response = await client.PutAsJsonAsync(
            $"api/orders/{id}",
            request);
        var problem = await ProblemDetailsAssert.ReadAsync<ProblemDetailsResponse>(
            response,
            "urn:backend-vsa-owin:problem:order-not-found");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Contains(id.ToString(), problem.Detail);
    }
}
