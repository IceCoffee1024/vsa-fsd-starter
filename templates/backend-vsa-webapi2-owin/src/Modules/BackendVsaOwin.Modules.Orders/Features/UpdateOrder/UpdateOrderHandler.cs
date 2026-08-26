using System;
using System.Threading;
using System.Threading.Tasks;
using BackendVsaOwin.Modules.Orders.Infrastructure;

namespace BackendVsaOwin.Modules.Orders.Features.UpdateOrder;

public sealed class UpdateOrderHandler
{
    private readonly IOrderStore _orders;

    internal UpdateOrderHandler(IOrderStore orders)
    {
        _orders = orders;
    }

    public async Task<UpdateOrderResponse?> HandleAsync(
        Guid id,
        UpdateOrderRequest request,
        CancellationToken cancellationToken)
    {
        var order = await _orders
            .UpdateAsync(
                id,
                request.TotalAmount,
                cancellationToken)
            .ConfigureAwait(false);

        return order is null
            ? null
            : new UpdateOrderResponse
            {
                Id = order.Id,
                CustomerId = order.CustomerId,
                CustomerName = order.CustomerName,
                TotalAmount = order.TotalAmount,
            };
    }
}
