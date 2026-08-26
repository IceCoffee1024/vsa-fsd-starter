using System;
using System.Collections.Generic;

namespace BackendVsaOwin.Modules.Orders.Features.BatchCreateOrders;

public sealed class BatchCreateOrdersResponse
{
    public BatchCreateOrdersResponse(
        IReadOnlyCollection<BatchCreatedOrderResponse> items)
    {
        CreatedCount = items.Count;
        Items = items;
    }

    public int CreatedCount { get; }

    public IReadOnlyCollection<BatchCreatedOrderResponse> Items { get; }
}

public sealed class BatchCreatedOrderResponse
{
    public required Guid Id { get; init; }

    public required Guid CustomerId { get; init; }

    public required string CustomerName { get; init; }

    public required decimal TotalAmount { get; init; }
}
