using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackendVsaOwin.Modules.Customers.Domain;
using BackendVsaOwin.Modules.Customers.Infrastructure;

namespace BackendVsaOwin.Modules.Customers.Tests.Support;

internal sealed class FakeCustomerStore : ICustomerStore
{
    private readonly Dictionary<Guid, Customer> _customers = new();

    public Task AddAsync(Customer customer, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _customers.Add(customer.Id, customer);
        return Task.CompletedTask;
    }

    public Task<Customer?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _customers.TryGetValue(id, out var customer);
        return Task.FromResult<Customer?>(customer);
    }

    public Task<IReadOnlyCollection<Customer>> GetManyAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyCollection<Customer> customers = ids
            .Distinct()
            .Where(_customers.ContainsKey)
            .Select(id => _customers[id])
            .ToArray();
        return Task.FromResult(customers);
    }
}
