using System;
using BackendVsaOwin.Modules.Orders.Domain;
using Microsoft.Data.Sqlite;
using Xunit;

namespace BackendVsaOwin.Modules.Orders.Tests.Infrastructure;

public sealed class SqliteOrderStoreTests
{
    [Fact]
    public async System.Threading.Tasks.Task Supports_the_complete_order_lifecycle_losslessly()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var database = new OrderSqliteDatabase();
        var id = Guid.Parse("51c67b21-883e-47cb-8b5f-bc5a7fc23067");
        var customerId = Guid.Parse("01d10ad9-ef17-4c98-9bca-a74baaa0f61d");
        const decimal originalAmount = 12345678901234567890.123456789m;
        const decimal updatedAmount = 0.0000000000000000000000000001m;
        await database.AddCustomerAsync(customerId, "Ada Lovelace", cancellationToken);

        await database.Store.AddAsync(
            new Order(id, customerId, "Ada Lovelace", originalAmount),
            cancellationToken);

        var created = await database.Store.GetAsync(id, cancellationToken);
        var listed = await database.Store.ListAsync(cancellationToken);
        var updated = await database.Store.UpdateAsync(
            id,
            updatedAmount,
            cancellationToken);
        var deleted = await database.Store.DeleteAsync(id, cancellationToken);
        var missing = await database.Store.GetAsync(id, cancellationToken);

        Assert.NotNull(created);
        Assert.Equal(originalAmount, created.TotalAmount);
        Assert.Equal(id, Assert.Single(listed).Id);
        Assert.NotNull(updated);
        Assert.Equal(customerId, updated.CustomerId);
        Assert.Equal("Ada Lovelace", updated.CustomerName);
        Assert.Equal(updatedAmount, updated.TotalAmount);
        Assert.True(deleted);
        Assert.Null(missing);
    }

    [Fact]
    public async System.Threading.Tasks.Task DeleteManyAsync_returns_only_deleted_ids()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var database = new OrderSqliteDatabase();
        var firstId = Guid.Parse("c6666344-2b7d-4b79-9a75-c782b5f80583");
        var secondId = Guid.Parse("ddfa93b2-7d8d-47a1-8806-bb4c29023d59");
        var missingId = Guid.Parse("f47b3295-e930-45a5-a238-c9eec98be4d8");
        var customerId = Guid.Parse("f66e1f67-c6b9-4af7-b722-1514641aeef3");
        await database.AddCustomerAsync(customerId, "Ada Lovelace", cancellationToken);
        await database.Store.AddAsync(
            new Order(firstId, customerId, "Ada Lovelace", 42.50m),
            cancellationToken);
        await database.Store.AddAsync(
            new Order(secondId, customerId, "Grace Hopper", 99.95m),
            cancellationToken);

        var deletedIds = await database.Store.DeleteManyAsync(
            new[] { secondId, missingId, firstId },
            cancellationToken);
        var remaining = await database.Store.ListAsync(cancellationToken);

        Assert.Equal(new[] { secondId, firstId }, deletedIds);
        Assert.Empty(remaining);
    }

    [Fact]
    public async System.Threading.Tasks.Task AddManyAsync_rolls_back_when_an_id_exists()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var database = new OrderSqliteDatabase();
        var existingId = Guid.Parse("17bb1398-a4a0-4277-82ad-a4058026759f");
        var newId = Guid.Parse("72f211f4-994d-42bc-a33a-4348e85dcc8f");
        var customerId = Guid.Parse("35e7bab4-f213-46c9-bf04-5970d8a1ee5b");
        await database.AddCustomerAsync(customerId, "Ada Lovelace", cancellationToken);
        await database.Store.AddAsync(
            new Order(existingId, customerId, "Ada Lovelace", 42.50m),
            cancellationToken);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => database.Store.AddManyAsync(
                new[]
                {
                    new Order(newId, customerId, "Grace Hopper", 99.95m),
                    new Order(existingId, customerId, "Margaret Hamilton", 125m),
                },
                cancellationToken));
        var orders = await database.Store.ListAsync(cancellationToken);

        var existing = Assert.Single(orders);
        Assert.Equal(existingId, existing.Id);
    }

    [Fact]
    public async System.Threading.Tasks.Task AddAsync_rejects_an_unknown_customer()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var database = new OrderSqliteDatabase();
        var order = new Order(
            Guid.Parse("3cafb921-0d9d-475e-baa2-925c6c07bf22"),
            Guid.Parse("93689c56-1974-4e32-8c09-68f40f662664"),
            "Unknown",
            42.50m);

        var exception = await Assert.ThrowsAsync<SqliteException>(
            () => database.Store.AddAsync(order, cancellationToken));

        Assert.Equal(19, exception.SqliteErrorCode);
        Assert.Empty(await database.Store.ListAsync(cancellationToken));
    }
}
