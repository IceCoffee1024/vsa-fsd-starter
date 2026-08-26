using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackendVsaOwin.Modules.Orders.Domain;

namespace BackendVsaOwin.Modules.Orders.Infrastructure;

internal sealed class InMemoryOrderStore : IOrderStore
{
    private readonly Dictionary<Guid, Order> _orders = new();
    private readonly object _syncRoot = new();

    public Task AddAsync(Order order, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_syncRoot)
        {
            if (_orders.ContainsKey(order.Id))
            {
                throw new InvalidOperationException($"Order '{order.Id}' already exists.");
            }

            _orders.Add(order.Id, order);
        }

        return Task.CompletedTask;
    }

    public Task AddManyAsync(
        IReadOnlyCollection<Order> orders,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_syncRoot)
        {
            if (orders.Select(order => order.Id).Distinct().Count() != orders.Count)
            {
                throw new InvalidOperationException("The batch contains duplicate order IDs.");
            }

            var existingOrder = orders.FirstOrDefault(order => _orders.ContainsKey(order.Id));
            if (existingOrder is not null)
            {
                throw new InvalidOperationException(
                    $"Order '{existingOrder.Id}' already exists.");
            }

            foreach (var order in orders)
            {
                _orders.Add(order.Id, order);
            }
        }

        return Task.CompletedTask;
    }

    public Task<Order?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Order? order;
        lock (_syncRoot)
        {
            _orders.TryGetValue(id, out order);
        }

        return Task.FromResult<Order?>(order);
    }

    public Task<IReadOnlyCollection<Order>> ListAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IReadOnlyCollection<Order> snapshot;
        lock (_syncRoot)
        {
            snapshot = _orders.Values
                .OrderBy(order => order.Id)
                .ToArray();
        }

        return Task.FromResult(snapshot);
    }

    public Task<Order?> UpdateAsync(
        Guid id,
        decimal totalAmount,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_syncRoot)
        {
            if (!_orders.ContainsKey(id))
            {
                return Task.FromResult<Order?>(null);
            }

            var existing = _orders[id];
            var updated = new Order(
                existing.Id,
                existing.CustomerId,
                existing.CustomerName,
                totalAmount);
            _orders[id] = updated;
            return Task.FromResult<Order?>(updated);
        }
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_syncRoot)
        {
            return Task.FromResult(_orders.Remove(id));
        }
    }

    public Task<IReadOnlyCollection<Guid>> DeleteManyAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var deletedIds = new List<Guid>(ids.Count);

        lock (_syncRoot)
        {
            foreach (var id in ids)
            {
                if (_orders.Remove(id))
                {
                    deletedIds.Add(id);
                }
            }
        }

        return Task.FromResult<IReadOnlyCollection<Guid>>(deletedIds);
    }
}
