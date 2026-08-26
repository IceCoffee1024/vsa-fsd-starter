using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackendVsaOwin.Modules.Orders.Infrastructure;

namespace BackendVsaOwin.Modules.Orders.Features.ListOrders;

public sealed class ListOrdersHandler
{
    private readonly IOrderStore _orders;

    internal ListOrdersHandler(IOrderStore orders)
    {
        _orders = orders;
    }

    public async Task<ListOrdersResponse> HandleAsync(
        CancellationToken cancellationToken)
    {
        var orders = await _orders
            .ListAsync(cancellationToken)
            .ConfigureAwait(false);

        var items = orders
            .Select(
                order => new OrderListItemResponse
                {
                    Id = order.Id,
                    CustomerId = order.CustomerId,
                    CustomerName = order.CustomerName,
                    TotalAmount = order.TotalAmount,
                })
            .ToArray();

        return new ListOrdersResponse { Items = items };
    }
}
