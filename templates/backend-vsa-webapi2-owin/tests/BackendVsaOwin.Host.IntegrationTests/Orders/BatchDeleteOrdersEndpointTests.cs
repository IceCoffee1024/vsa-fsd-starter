using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using BackendVsaOwin.Host.IntegrationTests.Support;
using BackendVsaOwin.Modules.Orders.Features;
using BackendVsaOwin.Modules.Orders.Features.BatchDeleteOrders;
using BackendVsaOwin.Modules.Orders.Features.ListOrders;
using BackendVsaOwin.BuildingBlocks.WebApi.Errors;
using Microsoft.Owin.Testing;
using Xunit;

namespace BackendVsaOwin.Host.IntegrationTests.Orders;

public sealed class BatchDeleteOrdersEndpointTests
{
    [Fact]
    public async Task Post_removes_existing_orders_and_reports_missing_ids()
    {
        using var server = TestServerFactory.Create();
        using var client = TestServerFactory.CreateAuthenticatedClient(server);
        var first = await OrderEndpointTestHelper.CreateOrderAsync(
            server,
            "Ada Lovelace",
            42.50m);
        var second = await OrderEndpointTestHelper.CreateOrderAsync(
            server,
            "Grace Hopper",
            99.95m);
        var missingId = Guid.Parse("eb50ae0f-dc10-482f-ae1b-a7979dd9beb3");
        var request = new BatchDeleteOrdersRequest
        {
            Ids = new[] { first.Id, missingId, second.Id },
        };

        using var batchResponse = await client.PostAsJsonAsync(
            "api/orders/batch-delete",
            request);
        var result = await batchResponse.Content.ReadAsAsync<BatchDeleteOrdersResponse>(
            TestContext.Current.CancellationToken);

        using var listResponse = await client.GetAsync(
            "api/orders",
            TestContext.Current.CancellationToken);
        var orders = await listResponse.Content.ReadAsAsync<ListOrdersResponse>(
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, batchResponse.StatusCode);
        Assert.Equal(3, result.RequestedCount);
        Assert.Equal(2, result.DeletedCount);
        Assert.Equal(missingId, Assert.Single(result.MissingIds));
        Assert.Empty(orders.Items);
    }

    [Fact]
    public async Task Post_rejects_an_empty_id_list()
    {
        using var server = TestServerFactory.Create();
        using var client = TestServerFactory.CreateAuthenticatedClient(server);
        var request = new BatchDeleteOrdersRequest
        {
            Ids = Array.Empty<Guid>(),
        };

        using var response = await client.PostAsJsonAsync(
            "api/orders/batch-delete",
            request);
        var problem = await ProblemDetailsAssert
            .ReadAsync<ValidationProblemDetailsResponse>(
                response,
                "urn:backend-vsa-owin:problem:validation-failed");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("ids", problem.Errors.Keys);
    }
}
