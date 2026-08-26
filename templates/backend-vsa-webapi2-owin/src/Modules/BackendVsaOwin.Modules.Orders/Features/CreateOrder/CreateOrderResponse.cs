using System;

namespace BackendVsaOwin.Modules.Orders.Features.CreateOrder;

public sealed class CreateOrderResponse
{
    public required Guid Id { get; init; }

    public required Guid CustomerId { get; init; }

    public required string CustomerName { get; init; }

    public required decimal TotalAmount { get; init; }
}
