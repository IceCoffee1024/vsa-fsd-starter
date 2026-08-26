using System;
using System.Threading;
using System.Threading.Tasks;
using BackendVsaOwin.Modules.Customers.Contracts;
using BackendVsaOwin.Modules.Orders.Features.CreateOrder;
using BackendVsaOwin.Modules.Orders.Tests.Support;
using Xunit;

namespace BackendVsaOwin.Modules.Orders.Tests.Features.CreateOrder;

public sealed class CreateOrderHandlerTests
{
    [Fact]
    public async Task HandleAsync_persists_and_returns_the_created_order()
    {
        var expectedId = Guid.Parse("b9c359d4-6e94-41ca-aa3c-0af90409d2a1");
        var customerId = Guid.Parse("9fc75e91-bc60-469d-98a6-d677e6760cd9");
        var store = new FakeOrderStore();
        var customers = new StubCustomerLookup(
            new CustomerReference(customerId, "Ada Lovelace"));
        var handler = new CreateOrderHandler(store, customers, () => expectedId);
        var request = new CreateOrderRequest
        {
            CustomerId = customerId,
            TotalAmount = 42.50m,
        };

        var result = await handler.HandleAsync(request, CancellationToken.None);
        var response = Assert.IsType<CreateOrderResponse>(result.Response);

        Assert.Equal(expectedId, response.Id);
        Assert.Equal(customerId, response.CustomerId);
        Assert.Equal("Ada Lovelace", response.CustomerName);
        Assert.Equal(42.50m, response.TotalAmount);
        var savedOrder = Assert.Single(
            await store.ListAsync(CancellationToken.None));
        Assert.Equal(expectedId, savedOrder.Id);
        Assert.Equal(customerId, savedOrder.CustomerId);
    }

    [Fact]
    public async Task HandleAsync_rejects_an_unknown_customer_without_persisting()
    {
        var store = new FakeOrderStore();
        var handler = new CreateOrderHandler(store, new StubCustomerLookup());
        var request = new CreateOrderRequest
        {
            CustomerId = Guid.Parse("e5682819-f2c2-4e3d-8f42-27ca739652bb"),
            TotalAmount = 42.50m,
        };

        var result = await handler.HandleAsync(request, CancellationToken.None);

        Assert.Null(result.Response);
        Assert.Contains("customerId", result.Errors.Keys);
        Assert.Empty(await store.ListAsync(CancellationToken.None));
    }
}
