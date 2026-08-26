using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackendVsaOwin.Modules.Customers.Domain;

namespace BackendVsaOwin.Modules.Customers.Infrastructure;

internal sealed class InMemoryCustomerStore : ICustomerStore
{
    private readonly Dictionary<Guid, Customer> _customers = new();
    private readonly object _syncRoot = new();

    public Task AddAsync(Customer customer, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_syncRoot)
        {
            if (_customers.ContainsKey(customer.Id))
            {
                throw new InvalidOperationException(
                    $"Customer '{customer.Id}' already exists.");
            }

            _customers.Add(customer.Id, customer);
        }

        return Task.CompletedTask;
    }

    public Task<Customer?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Customer? customer;
        lock (_syncRoot)
        {
            _customers.TryGetValue(id, out customer);
        }

        return Task.FromResult<Customer?>(customer);
    }

    public Task<IReadOnlyCollection<Customer>> GetManyAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IReadOnlyCollection<Customer> customers;
        lock (_syncRoot)
        {
            customers = ids
                .Distinct()
                .Where(_customers.ContainsKey)
                .Select(id => _customers[id])
                .ToArray();
        }

        return Task.FromResult(customers);
    }
}
