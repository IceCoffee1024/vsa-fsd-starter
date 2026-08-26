using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BackendVsaOwin.Modules.Orders.Domain;

namespace BackendVsaOwin.Modules.Orders.Infrastructure;

internal interface IOrderStore
{
    Task AddAsync(Order order, CancellationToken cancellationToken);

    Task AddManyAsync(
        IReadOnlyCollection<Order> orders,
        CancellationToken cancellationToken);

    Task<Order?> GetAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Order>> ListAsync(CancellationToken cancellationToken);

    Task<Order?> UpdateAsync(
        Guid id,
        decimal totalAmount,
        CancellationToken cancellationToken);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Guid>> DeleteManyAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken);
}
