using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BackendVsaOwin.Modules.Customers.Domain;

namespace BackendVsaOwin.Modules.Customers.Infrastructure;

internal interface ICustomerStore
{
    Task AddAsync(Customer customer, CancellationToken cancellationToken);

    Task<Customer?> GetAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Customer>> GetManyAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken);
}
