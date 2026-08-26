using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackendVsaOwin.Modules.Customers.Contracts;
using BackendVsaOwin.Modules.Orders.Domain;
using BackendVsaOwin.Modules.Orders.Infrastructure;

namespace BackendVsaOwin.Modules.Orders.Features.BatchCreateOrders;

public sealed class BatchCreateOrdersHandler
{
    private readonly Func<Guid> _createId;
    private readonly ICustomerLookup _customers;
    private readonly IOrderStore _orders;

    internal BatchCreateOrdersHandler(
        IOrderStore orders,
        ICustomerLookup customers,
        Func<Guid>? createId = null)
    {
        _orders = orders;
        _customers = customers;
        _createId = createId ?? Guid.NewGuid;
    }

    public async Task<BatchCreateOrdersResult> HandleAsync(
        BatchCreateOrdersRequest request,
        CancellationToken cancellationToken)
    {
        var requestItems = request.Orders!;
        var customers = await _customers.FindManyAsync(
            requestItems
                .Select(item => item!.CustomerId)
                .Distinct()
                .ToArray(),
            cancellationToken);
        var errors = new Dictionary<string, string[]>();

        for (var index = 0; index < requestItems.Length; index++)
        {
            if (!customers.ContainsKey(requestItems[index]!.CustomerId))
            {
                errors[$"orders[{index}].customerId"] = new[]
                {
                    "The specified customer does not exist.",
                };
            }
        }

        if (errors.Count > 0)
        {
            return BatchCreateOrdersResult.Failure(errors);
        }

        var orders = new List<Order>(requestItems.Length);
        var responseItems = new List<BatchCreatedOrderResponse>(requestItems.Length);

        foreach (var item in requestItems)
        {
            var customer = customers[item!.CustomerId];
            var order = new Order(
                _createId(),
                customer.Id,
                customer.DisplayName,
                item.TotalAmount);
            orders.Add(order);
            responseItems.Add(
                new BatchCreatedOrderResponse
                {
                    Id = order.Id,
                    CustomerId = order.CustomerId,
                    CustomerName = order.CustomerName,
                    TotalAmount = order.TotalAmount,
                });
        }

        await _orders.AddManyAsync(orders, cancellationToken);

        return BatchCreateOrdersResult.Success(
            new BatchCreateOrdersResponse(responseItems));
    }
}
