using System;

namespace BackendVsaOwin.Modules.Orders.Features.BatchDeleteOrders;

public sealed class BatchDeleteOrdersRequest
{
    /// <summary>
    /// Unique, non-empty order identifiers; must contain between 1 and 100 values.
    /// </summary>
    public Guid[]? Ids { get; set; }
}
