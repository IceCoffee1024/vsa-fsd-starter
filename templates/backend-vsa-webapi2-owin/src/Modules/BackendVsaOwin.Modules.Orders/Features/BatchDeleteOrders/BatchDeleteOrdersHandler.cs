using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackendVsaOwin.Modules.Orders.Infrastructure;

namespace BackendVsaOwin.Modules.Orders.Features.BatchDeleteOrders;

public sealed class BatchDeleteOrdersHandler
{
    private readonly IOrderStore _orders;

    internal BatchDeleteOrdersHandler(IOrderStore orders)
    {
        _orders = orders;
    }

    public async Task<BatchDeleteOrdersResponse> HandleAsync(
        BatchDeleteOrdersRequest request,
        CancellationToken cancellationToken)
    {
        var ids = request.Ids!;
        var deletedIds = await _orders.DeleteManyAsync(ids, cancellationToken);
        var deletedIdSet = new HashSet<System.Guid>(deletedIds);
        var missingIds = ids
            .Where(id => !deletedIdSet.Contains(id))
            .ToArray();

        return new BatchDeleteOrdersResponse(
            ids.Length,
            deletedIds.Count,
            missingIds);
    }
}
