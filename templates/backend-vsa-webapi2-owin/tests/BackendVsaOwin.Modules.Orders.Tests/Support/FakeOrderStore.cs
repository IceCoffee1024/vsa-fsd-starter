using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackendVsaOwin.Modules.Orders.Domain;
using BackendVsaOwin.Modules.Orders.Infrastructure;

namespace BackendVsaOwin.Modules.Orders.Tests.Support;

internal sealed class FakeOrderStore : IOrderStore
{
    private readonly Dictionary<Guid, Order> _orders = new();

    public Task AddAsync(Order order, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _orders.Add(order.Id, order);
        return Task.CompletedTask;
    }

    public Task AddManyAsync(
        IReadOnlyCollection<Order> orders,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        foreach (var order in orders)
        {
            _orders.Add(order.Id, order);
        }

        return Task.CompletedTask;
    }

    public Task<Order?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _orders.TryGetValue(id, out var order);
        return Task.FromResult<Order?>(order);
    }

    public Task<IReadOnlyCollection<Order>> ListAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyCollection<Order>>(
            _orders.Values.OrderBy(order => order.Id).ToArray());
    }

    public Task<Order?> UpdateAsync(
        Guid id,
        decimal totalAmount,
        CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    public Task<IReadOnlyCollection<Guid>> DeleteManyAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }
}
