using System;

namespace BackendVsaOwin.Modules.Orders.Domain;

internal sealed class Order
{
    public Order(
        Guid id,
        Guid customerId,
        string customerName,
        decimal totalAmount)
    {
        Id = id;
        CustomerId = customerId;
        CustomerName = customerName;
        TotalAmount = totalAmount;
    }

    public Guid Id { get; }

    public Guid CustomerId { get; }

    public string CustomerName { get; }

    public decimal TotalAmount { get; }
}
