using System;
using System.Collections.Generic;

namespace BackendVsaOwin.Modules.Orders.Features.ListOrders;

public sealed class ListOrdersResponse
{
    public required IReadOnlyCollection<OrderListItemResponse> Items { get; init; }
}

public sealed class OrderListItemResponse
{
    public required Guid Id { get; init; }

    public required Guid CustomerId { get; init; }

    public required string CustomerName { get; init; }

    public required decimal TotalAmount { get; init; }
}
