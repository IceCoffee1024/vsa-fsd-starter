using System;
using BackendVsaOwin.Modules.Customers.Features.CreateCustomer;
using BackendVsaOwin.Modules.Customers.Infrastructure;
using BackendVsaOwin.Modules.Customers.Tests.Support;
using Xunit;

namespace BackendVsaOwin.Modules.Customers.Tests.Features.CreateCustomer;

public sealed class CreateCustomerHandlerTests
{
    [Fact]
    public async System.Threading.Tasks.Task HandleAsync_persists_a_trimmed_customer()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var expectedId = Guid.Parse("8de376e5-e9ac-4870-8469-94eb45e25ae2");
        var store = new FakeCustomerStore();
        var handler = new CreateCustomerHandler(store, () => expectedId);
        var request = new CreateCustomerRequest
        {
            DisplayName = "  Ada Lovelace  ",
        };

        var response = await handler.HandleAsync(request, cancellationToken);
        var stored = await store.GetAsync(expectedId, cancellationToken);

        Assert.Equal(expectedId, response.Id);
        Assert.Equal("Ada Lovelace", response.DisplayName);
        Assert.NotNull(stored);
        Assert.Equal("Ada Lovelace", stored.DisplayName);
    }
}
