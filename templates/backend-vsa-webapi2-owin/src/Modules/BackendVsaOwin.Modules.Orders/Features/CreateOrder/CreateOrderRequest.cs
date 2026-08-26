using System;

namespace BackendVsaOwin.Modules.Orders.Features.CreateOrder;

public sealed class CreateOrderRequest
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
