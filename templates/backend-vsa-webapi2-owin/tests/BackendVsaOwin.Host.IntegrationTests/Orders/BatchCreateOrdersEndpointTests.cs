using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using BackendVsaOwin.Host.IntegrationTests.Support;
using BackendVsaOwin.Modules.Orders.Features;
using BackendVsaOwin.Modules.Orders.Features.BatchCreateOrders;
using BackendVsaOwin.Modules.Orders.Features.ListOrders;
using BackendVsaOwin.Host.IntegrationTests.Customers;
using BackendVsaOwin.BuildingBlocks.WebApi.Errors;
using Microsoft.Owin.Testing;
using Xunit;

namespace BackendVsaOwin.Host.IntegrationTests.Orders;

public sealed class BatchCreateOrdersEndpointTests
{
    [Fact]
    public async Task Post_persists_and_returns_every_order()
    {
        using var server = TestServerFactory.Create();
        using var client = TestServerFactory.CreateAuthenticatedClient(server);
        var firstCustomer = await CustomerEndpointTestHelper.CreateCustomerAsync(
            server,
            "Ada Lovelace");
        var secondCustomer = await CustomerEndpointTestHelper.CreateCustomerAsync(
            server,
            "Grace Hopper");
        var request = new BatchCreateOrdersRequest
        {
            Orders = new BatchCreateOrderItem?[]
            {
                new()
                {
                    CustomerId = firstCustomer.Id,
                    TotalAmount = 42.50m,
                },
                new()
                {
                    CustomerId = secondCustomer.Id,
                    TotalAmount = 99.95m,
                },
            },
        };

        using var createResponse = await client.PostAsJsonAsync(
            "api/orders/batch",
            request);
        var result = await createResponse.Content.ReadAsAsync<BatchCreateOrdersResponse>(
            TestContext.Current.CancellationToken);

        using var listResponse = await client.GetAsync(
            "api/orders",
            TestContext.Current.CancellationToken);
        var orders = await listResponse.Content.ReadAsAsync<ListOrdersResponse>(
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        Assert.Equal(2, result.CreatedCount);
        Assert.Equal(2, result.Items.Count);
        Assert.Contains(result.Items, item => item.CustomerName == "Ada Lovelace");
        Assert.Equal(2, orders.Items.Count);
    }

    [Fact]
    public async Task Post_rejects_the_entire_invalid_batch()
    {
        using var server = TestServerFactory.Create();
        using var client = TestServerFactory.CreateAuthenticatedClient(server);
        var customer = await CustomerEndpointTestHelper.CreateCustomerAsync(
            server,
            "Ada Lovelace");
        var request = new BatchCreateOrdersRequest
        {
            Orders = new BatchCreateOrderItem?[]
            {
                new()
                {
                    CustomerId = customer.Id,
                    TotalAmount = 42.50m,
                },
                new()
                {
                    CustomerId = Guid.Empty,
                    TotalAmount = 0,
                },
            },
        };

        using var createResponse = await client.PostAsJsonAsync(
            "api/orders/batch",
            request);
        var problem = await ProblemDetailsAssert
            .ReadAsync<ValidationProblemDetailsResponse>(
                createResponse,
                "urn:backend-vsa-owin:problem:validation-failed");

        using var listResponse = await client.GetAsync(
            "api/orders",
            TestContext.Current.CancellationToken);
        var orders = await listResponse.Content.ReadAsAsync<ListOrdersResponse>(
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, createResponse.StatusCode);
        Assert.Contains("orders[1].customerId", problem.Errors.Keys);
        Assert.Contains("orders[1].totalAmount", problem.Errors.Keys);
        Assert.Empty(orders.Items);
    }

    [Fact]
    public async Task Post_rejects_the_entire_batch_when_a_customer_is_unknown()
    {
        using var server = TestServerFactory.Create();
        using var client = TestServerFactory.CreateAuthenticatedClient(server);
        var customer = await CustomerEndpointTestHelper.CreateCustomerAsync(
            server,
            "Ada Lovelace");
        var request = new BatchCreateOrdersRequest
        {
            Orders = new BatchCreateOrderItem?[]
            {
                new() { CustomerId = customer.Id, TotalAmount = 42.50m },
                new()
                {
                    CustomerId = Guid.Parse("8f10ac73-e5ef-40aa-8711-f6750e0162a8"),
                    TotalAmount = 99.95m,
                },
            },
        };

        using var createResponse = await client.PostAsJsonAsync(
            "api/orders/batch",
            request);
        var problem = await ProblemDetailsAssert
            .ReadAsync<ValidationProblemDetailsResponse>(
                createResponse,
                "urn:backend-vsa-owin:problem:validation-failed");
        using var listResponse = await client.GetAsync(
            "api/orders",
            TestContext.Current.CancellationToken);
        var orders = await listResponse.Content.ReadAsAsync<ListOrdersResponse>(
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, createResponse.StatusCode);
        Assert.Contains("orders[1].customerId", problem.Errors.Keys);
        Assert.Empty(orders.Items);
    }
}
