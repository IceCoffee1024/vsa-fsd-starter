using System;
using BackendVsaOwin.Modules.Customers.Domain;
using BackendVsaOwin.Modules.Customers.Infrastructure;
using Xunit;

namespace BackendVsaOwin.Modules.Customers.Tests.Infrastructure;

public sealed class CustomerLookupTests
{
    [Fact]
    public async System.Threading.Tasks.Task FindManyAsync_returns_only_existing_customers()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var existingId = Guid.Parse("2101bbd1-c7b0-4e56-a779-268210ab4b89");
        var missingId = Guid.Parse("e3be5ca9-1a23-4115-bfe3-b9f60656f2bb");
        var store = new InMemoryCustomerStore();
        await store.AddAsync(
            new Customer(existingId, "Ada Lovelace"),
            cancellationToken);
        var lookup = new CustomerLookup(store);

        var customers = await lookup.FindManyAsync(
            new[] { existingId, missingId },
            cancellationToken);

        var customer = Assert.Single(customers);
        Assert.Equal(existingId, customer.Key);
        Assert.Equal("Ada Lovelace", customer.Value.DisplayName);
    }
}
