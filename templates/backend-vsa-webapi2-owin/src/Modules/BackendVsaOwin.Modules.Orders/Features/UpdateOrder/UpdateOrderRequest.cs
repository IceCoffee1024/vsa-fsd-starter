namespace BackendVsaOwin.Modules.Orders.Features.UpdateOrder;

public sealed class UpdateOrderRequest
{
    /// <summary>
    /// Total order amount; must be greater than zero.
    /// </summary>
    public decimal TotalAmount { get; set; }
}
