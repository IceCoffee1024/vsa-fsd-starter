using System;

namespace BackendVsaOwin.Modules.Orders.Features.UpdateOrder;

public sealed class UpdateOrderResponse
{
    public required Guid Id { get; init; }

    public required Guid CustomerId { get; init; }

    public required string CustomerName { get; init; }

    public required decimal TotalAmount { get; init; }
}
