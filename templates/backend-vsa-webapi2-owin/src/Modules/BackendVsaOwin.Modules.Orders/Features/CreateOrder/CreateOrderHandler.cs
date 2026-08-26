using System;
using System.Threading;
using System.Threading.Tasks;
using BackendVsaOwin.Modules.Customers.Contracts;
using BackendVsaOwin.Modules.Orders.Domain;
using BackendVsaOwin.Modules.Orders.Infrastructure;

namespace BackendVsaOwin.Modules.Orders.Features.CreateOrder;

public sealed class CreateOrderHandler
{
    private readonly Func<Guid> _createId;
    private readonly ICustomerLookup _customers;
    private readonly IOrderStore _orders;

    internal CreateOrderHandler(
        IOrderStore orders,
        ICustomerLookup customers,
        Func<Guid>? createId = null)
    {
        _orders = orders;
        _customers = customers;
        _createId = createId ?? Guid.NewGuid;
    }

    public async Task<CreateOrderResult> HandleAsync(
        CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        var customer = await _customers
            .FindAsync(request.CustomerId, cancellationToken)
            .ConfigureAwait(false);
        if (customer is null)
        {
            return CreateOrderResult.Failure(
                "customerId",
                "The specified customer does not exist.");
        }

        var order = new Order(
            _createId(),
            customer.Id,
            customer.DisplayName,
            request.TotalAmount);

        await _orders.AddAsync(order, cancellationToken).ConfigureAwait(false);

        return CreateOrderResult.Success(
            new CreateOrderResponse
            {
                Id = order.Id,
                CustomerId = order.CustomerId,
                CustomerName = order.CustomerName,
                TotalAmount = order.TotalAmount,
            });
    }
}
