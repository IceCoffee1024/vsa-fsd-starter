using System;
using System.Threading;
using System.Threading.Tasks;
using BackendVsaOwin.Modules.Orders.Infrastructure;

namespace BackendVsaOwin.Modules.Orders.Features.DeleteOrder;

public sealed class DeleteOrderHandler
{
    private readonly IOrderStore _orders;

    internal DeleteOrderHandler(IOrderStore orders)
    {
        _orders = orders;
    }

    public Task<bool> HandleAsync(Guid id, CancellationToken cancellationToken)
    {
        return _orders.DeleteAsync(id, cancellationToken);
    }
}
