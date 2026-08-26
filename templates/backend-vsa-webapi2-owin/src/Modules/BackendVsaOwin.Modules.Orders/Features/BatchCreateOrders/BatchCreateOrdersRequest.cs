using System;

namespace BackendVsaOwin.Modules.Orders.Features.BatchCreateOrders;

public sealed class BatchCreateOrdersRequest
{
    /// <summary>
    /// Orders to create; must contain between 1 and 100 items.
    /// </summary>
    public BatchCreateOrderItem?[]? Orders { get; set; }
}

public sealed class BatchCreateOrderItem
{
    /// <summary>
    /// Existing customer identifier; the order captures the customer's current display name.
    /// </summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// Total order amount; must be greater than zero.
    /// </summary>
    public decimal TotalAmount { get; set; }
}
