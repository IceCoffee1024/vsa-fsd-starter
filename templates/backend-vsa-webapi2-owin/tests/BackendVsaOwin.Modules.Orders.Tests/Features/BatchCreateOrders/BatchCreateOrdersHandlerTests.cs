using System;
using System.Collections.Generic;
using BackendVsaOwin.Modules.Customers.Contracts;
using BackendVsaOwin.Modules.Orders.Features.BatchCreateOrders;
using BackendVsaOwin.Modules.Orders.Infrastructure;
using BackendVsaOwin.Modules.Orders.Tests.Support;
using Xunit;

namespace BackendVsaOwin.Modules.Orders.Tests.Features.BatchCreateOrders;

public sealed class BatchCreateOrdersHandlerTests
{
    [Fact]
    public async System.Threading.Tasks.Task HandleAsync_persists_the_complete_batch()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var firstId = Guid.Parse("3180180f-fcfa-484a-8e51-d18875737392");
        var secondId = Guid.Parse("6e8ca746-7b70-49bf-8a35-0d387df46ad7");
        var firstCustomerId = Guid.Parse("723e4e68-1936-4d0d-aa6e-3e8cf487cfb4");
        var secondCustomerId = Guid.Parse("3a3dfe2a-ee43-4913-af69-dd5685df26fd");
        var ids = new Queue<Guid>(new[] { firstId, secondId });
        var store = new FakeOrderStore();
        var customers = new StubCustomerLookup(
            new CustomerReference(firstCustomerId, "Ada Lovelace"),
            new CustomerReference(secondCustomerId, "Grace Hopper"));
        var handler = new BatchCreateOrdersHandler(store, customers, ids.Dequeue);
        var request = new BatchCreateOrdersRequest
        {
            Orders = new BatchCreateOrderItem?[]
            {
                new()
                {
                    CustomerId = firstCustomerId,
                    TotalAmount = 42.50m,
                },
                new()
                {
                    CustomerId = secondCustomerId,
                    TotalAmount = 99.95m,
                },
            },
        };

        var result = await handler.HandleAsync(request, cancellationToken);
        var response = Assert.IsType<BatchCreateOrdersResponse>(result.Response);
        var storedOrders = await store.ListAsync(cancellationToken);

        Assert.Equal(2, response.CreatedCount);
        Assert.Collection(
            response.Items,
            item =>
            {
                Assert.Equal(firstId, item.Id);
                Assert.Equal(firstCustomerId, item.CustomerId);
                Assert.Equal("Ada Lovelace", item.CustomerName);
            },
            item => Assert.Equal(secondId, item.Id));
        Assert.Equal(2, storedOrders.Count);
    }

    [Fact]
    public async System.Threading.Tasks.Task HandleAsync_rejects_the_batch_when_a_customer_is_missing()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var existingCustomerId = Guid.Parse("7cc6451d-c7c4-4424-b86f-2ad66b2273c9");
        var missingCustomerId = Guid.Parse("920e702c-5a8b-4a64-a71f-74d5399f0116");
        var store = new FakeOrderStore();
        var customers = new StubCustomerLookup(
            new CustomerReference(existingCustomerId, "Ada Lovelace"));
        var handler = new BatchCreateOrdersHandler(store, customers);
        var request = new BatchCreateOrdersRequest
        {
            Orders = new BatchCreateOrderItem?[]
            {
                new() { CustomerId = existingCustomerId, TotalAmount = 42.50m },
                new() { CustomerId = missingCustomerId, TotalAmount = 99.95m },
            },
        };

        var result = await handler.HandleAsync(request, cancellationToken);
        var storedOrders = await store.ListAsync(cancellationToken);

        Assert.Null(result.Response);
        Assert.Contains("orders[1].customerId", result.Errors.Keys);
        Assert.Empty(storedOrders);
    }
}
