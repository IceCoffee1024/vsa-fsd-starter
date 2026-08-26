using System;
using System.Collections.Generic;

namespace BackendVsaOwin.Modules.Orders.Features.BatchDeleteOrders;

public sealed class BatchDeleteOrdersResponse
{
    public BatchDeleteOrdersResponse(
        int requestedCount,
        int deletedCount,
        IReadOnlyCollection<Guid> missingIds)
    {
        RequestedCount = requestedCount;
        DeletedCount = deletedCount;
        MissingIds = missingIds;
    }

    /// <summary>
    /// Number of identifiers supplied in the request.
    /// </summary>
    public int RequestedCount { get; }

    /// <summary>
    /// Number of orders that existed and were deleted.
    /// </summary>
    public int DeletedCount { get; }

    /// <summary>
    /// Identifiers that did not match an existing order.
    /// </summary>
    public IReadOnlyCollection<Guid> MissingIds { get; }
}
