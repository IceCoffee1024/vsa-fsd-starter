using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackendVsaOwin.Modules.Customers.Contracts;

namespace BackendVsaOwin.Modules.Orders.Tests.Support;

internal sealed class StubCustomerLookup : ICustomerLookup
{
    private readonly IReadOnlyDictionary<Guid, CustomerReference> _customers;

    public StubCustomerLookup(params CustomerReference[] customers)
    {
        _customers = customers.ToDictionary(customer => customer.Id);
    }

    public Task<CustomerReference?> FindAsync(
        Guid customerId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _customers.TryGetValue(customerId, out var customer);
        return Task.FromResult<CustomerReference?>(customer);
    }

    public Task<IReadOnlyDictionary<Guid, CustomerReference>> FindManyAsync(
        IReadOnlyCollection<Guid> customerIds,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyDictionary<Guid, CustomerReference> result = customerIds
            .Distinct()
            .Where(_customers.ContainsKey)
            .ToDictionary(id => id, id => _customers[id]);
        return Task.FromResult(result);
    }
}
