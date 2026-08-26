using System;
using BackendVsaOwin.Modules.Customers.Domain;
using Xunit;

namespace BackendVsaOwin.Modules.Customers.Tests.Infrastructure;

public sealed class SqliteCustomerStoreTests
{
    [Fact]
    public async System.Threading.Tasks.Task Supports_customer_storage_and_batch_lookup()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var database = new CustomerSqliteDatabase();
        var firstId = Guid.Parse("8de376e5-e9ac-4870-8469-94eb45e25ae2");
        var secondId = Guid.Parse("2101bbd1-c7b0-4e56-a779-268210ab4b89");
        var missingId = Guid.Parse("e3be5ca9-1a23-4115-bfe3-b9f60656f2bb");

        await database.Store.AddAsync(
            new Customer(firstId, "Ada Lovelace"),
            cancellationToken);
        await database.Store.AddAsync(
            new Customer(secondId, "Grace Hopper"),
            cancellationToken);

        var first = await database.Store.GetAsync(firstId, cancellationToken);
        var customers = await database.Store.GetManyAsync(
            new[] { secondId, missingId, firstId, secondId },
            cancellationToken);

        Assert.NotNull(first);
        Assert.Equal("Ada Lovelace", first.DisplayName);
        Assert.Collection(
            customers,
            customer => Assert.Equal(secondId, customer.Id),
            customer => Assert.Equal(firstId, customer.Id));
    }
}
