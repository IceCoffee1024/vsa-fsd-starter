using System;
using System.Threading;
using System.Threading.Tasks;
using BackendVsaOwin.Modules.Orders.Infrastructure;

namespace BackendVsaOwin.Modules.Orders.Features.GetOrder;

public sealed class GetOrderHandler
{
    private readonly IOrderStore _orders;

    internal GetOrderHandler(IOrderStore orders)
    {
        _orders = orders;
    }

    public async Task<GetOrderResponse?> HandleAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var order = await _orders
            .GetAsync(id, cancellationToken)
            .ConfigureAwait(false);

        return order is null
            ? null
            : new GetOrderResponse
            {
                Id = order.Id,
                CustomerId = order.CustomerId,
                CustomerName = order.CustomerName,
                TotalAmount = order.TotalAmount,
            };
    }
}
