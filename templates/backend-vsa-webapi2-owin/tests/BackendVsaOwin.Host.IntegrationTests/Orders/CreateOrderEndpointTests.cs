using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using BackendVsaOwin.Host.IntegrationTests.Support;
using BackendVsaOwin.Modules.Orders.Features;
using BackendVsaOwin.Modules.Orders.Features.CreateOrder;
using BackendVsaOwin.Host.IntegrationTests.Customers;
using BackendVsaOwin.BuildingBlocks.WebApi.Errors;
using Microsoft.Owin.Testing;
using Xunit;

namespace BackendVsaOwin.Host.IntegrationTests.Orders;

public sealed class CreateOrderEndpointTests
{
    [Fact]
    public async Task Post_returns_created_order_for_a_valid_request()
    {
        using var server = TestServerFactory.Create();
        using var client = TestServerFactory.CreateAuthenticatedClient(server);
        var customer = await CustomerEndpointTestHelper.CreateCustomerAsync(
            server,
            "Ada Lovelace");
        var request = new CreateOrderRequest
        {
            CustomerId = customer.Id,
            TotalAmount = 42.50m,
        };

        using var response = await client.PostAsJsonAsync("api/orders", request);
        var body = await response.Content.ReadAsAsync<CreateOrderResponse>(
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal($"/api/orders/{body.Id}", response.Headers.Location?.AbsolutePath);
        Assert.NotEqual(Guid.Empty, body.Id);
        Assert.Equal(customer.Id, body.CustomerId);
        Assert.Equal("Ada Lovelace", body.CustomerName);
        Assert.Equal(42.50m, body.TotalAmount);
    }

    [Fact]
    public async Task Post_returns_validation_problem_for_an_invalid_request()
    {
        using var server = TestServerFactory.Create();
        using var client = TestServerFactory.CreateAuthenticatedClient(server);
        var request = new CreateOrderRequest
        {
            CustomerId = Guid.Empty,
            TotalAmount = -1,
        };

        using var response = await client.PostAsJsonAsync("api/orders", request);
        var body = await ProblemDetailsAssert
            .ReadAsync<ValidationProblemDetailsResponse>(
                response,
                "urn:backend-vsa-owin:problem:validation-failed");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("customerId", body.Errors.Keys);
        Assert.Contains("totalAmount", body.Errors.Keys);
    }

    [Fact]
    public async Task Post_rejects_an_unknown_customer()
    {
        using var server = TestServerFactory.Create();
        using var client = TestServerFactory.CreateAuthenticatedClient(server);
        var request = new CreateOrderRequest
        {
            CustomerId = Guid.Parse("e5682819-f2c2-4e3d-8f42-27ca739652bb"),
            TotalAmount = 42.50m,
        };

        using var response = await client.PostAsJsonAsync("api/orders", request);
        var body = await ProblemDetailsAssert
            .ReadAsync<ValidationProblemDetailsResponse>(
                response,
                "urn:backend-vsa-owin:problem:validation-failed");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("customerId", body.Errors.Keys);
    }
}
