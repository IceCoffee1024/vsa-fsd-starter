using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackendVsaOwin.Modules.Customers.Contracts;

namespace BackendVsaOwin.Modules.Customers.Infrastructure;

internal sealed class CustomerLookup : ICustomerLookup
{
    private readonly ICustomerStore _customers;

    public CustomerLookup(ICustomerStore customers)
    {
        _customers = customers;
    }

    public async Task<CustomerReference?> FindAsync(
        Guid customerId,
        CancellationToken cancellationToken)
    {
        var customer = await _customers.GetAsync(customerId, cancellationToken);

        return customer is null
            ? null
            : new CustomerReference(customer.Id, customer.DisplayName);
    }

    public async Task<IReadOnlyDictionary<Guid, CustomerReference>> FindManyAsync(
        IReadOnlyCollection<Guid> customerIds,
        CancellationToken cancellationToken)
    {
        var customers = await _customers.GetManyAsync(customerIds, cancellationToken);

        return customers.ToDictionary(
            customer => customer.Id,
            customer => new CustomerReference(customer.Id, customer.DisplayName));
    }
}
