using System;
using System.Linq;
using BackendVsaOwin.Modules.Orders.Domain;
using BackendVsaOwin.Modules.Orders.Infrastructure;
using Xunit;

namespace BackendVsaOwin.Modules.Orders.Tests.Infrastructure;

public sealed class InMemoryOrderStoreTests
{
    [Fact]
    public async System.Threading.Tasks.Task Supports_the_complete_order_lifecycle()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var store = new InMemoryOrderStore();
        var id = Guid.Parse("51c67b21-883e-47cb-8b5f-bc5a7fc23067");
        var customerId = Guid.Parse("01d10ad9-ef17-4c98-9bca-a74baaa0f61d");

        await store.AddAsync(
            new Order(id, customerId, "Ada Lovelace", 42.50m),
            cancellationToken);

        var created = await store.GetAsync(id, cancellationToken);
        var listed = await store.ListAsync(cancellationToken);
        var updated = await store.UpdateAsync(
            id,
            99.95m,
            cancellationToken);
        var deleted = await store.DeleteAsync(id, cancellationToken);
        var missing = await store.GetAsync(id, cancellationToken);

        Assert.NotNull(created);
        Assert.Equal(id, Assert.Single(listed).Id);
        Assert.NotNull(updated);
        Assert.Equal(customerId, updated.CustomerId);
        Assert.Equal("Ada Lovelace", updated.CustomerName);
        Assert.Equal(99.95m, updated.TotalAmount);
        Assert.True(deleted);
        Assert.Null(missing);
    }

    [Fact]
    public async System.Threading.Tasks.Task DeleteManyAsync_returns_only_deleted_ids()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var store = new InMemoryOrderStore();
        var firstId = Guid.Parse("c6666344-2b7d-4b79-9a75-c782b5f80583");
        var secondId = Guid.Parse("ddfa93b2-7d8d-47a1-8806-bb4c29023d59");
        var missingId = Guid.Parse("f47b3295-e930-45a5-a238-c9eec98be4d8");
        var customerId = Guid.Parse("f66e1f67-c6b9-4af7-b722-1514641aeef3");

        await store.AddAsync(
            new Order(firstId, customerId, "Ada Lovelace", 42.50m),
            cancellationToken);
        await store.AddAsync(
            new Order(secondId, customerId, "Grace Hopper", 99.95m),
            cancellationToken);

        var deletedIds = await store.DeleteManyAsync(
            new[] { secondId, missingId, firstId },
            cancellationToken);
        var remaining = await store.ListAsync(cancellationToken);

        Assert.Equal(new[] { secondId, firstId }, deletedIds);
        Assert.Empty(remaining);
    }

    [Fact]
    public async System.Threading.Tasks.Task AddManyAsync_is_atomic_when_an_id_exists()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var store = new InMemoryOrderStore();
        var existingId = Guid.Parse("17bb1398-a4a0-4277-82ad-a4058026759f");
        var newId = Guid.Parse("72f211f4-994d-42bc-a33a-4348e85dcc8f");
        var customerId = Guid.Parse("35e7bab4-f213-46c9-bf04-5970d8a1ee5b");
        await store.AddAsync(
            new Order(existingId, customerId, "Ada Lovelace", 42.50m),
            cancellationToken);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => store.AddManyAsync(
                new[]
                {
                    new Order(newId, customerId, "Grace Hopper", 99.95m),
                    new Order(existingId, customerId, "Margaret Hamilton", 125m),
                },
                cancellationToken));
        var orders = await store.ListAsync(cancellationToken);

        var existing = Assert.Single(orders);
        Assert.Equal(existingId, existing.Id);
        Assert.Equal("Ada Lovelace", existing.CustomerName);
    }
}
