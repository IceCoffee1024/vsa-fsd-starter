using System;
using System.Collections.Generic;

namespace BackendVsaOwin.Modules.Orders.Features.BatchCreateOrders;

public sealed class BatchCreateOrdersValidator
{
    private const int MaximumBatchSize = 100;

    public IReadOnlyDictionary<string, string[]> Validate(
        BatchCreateOrdersRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (request.Orders is null || request.Orders.Length == 0)
        {
            errors["orders"] = new[] { "At least one order is required." };
            return errors;
        }

        if (request.Orders.Length > MaximumBatchSize)
        {
            errors["orders"] = new[]
            {
                $"A maximum of {MaximumBatchSize} orders is allowed.",
            };
            return errors;
        }

        for (var index = 0; index < request.Orders.Length; index++)
        {
            var order = request.Orders[index];
            if (order is null)
            {
                errors[$"orders[{index}]"] = new[] { "An order is required." };
                continue;
            }

            if (order.CustomerId == Guid.Empty)
            {
                errors[$"orders[{index}].customerId"] = new[]
                {
                    "Customer ID is required.",
                };
            }

            if (order.TotalAmount <= 0)
            {
                errors[$"orders[{index}].totalAmount"] = new[]
                {
                    "Total amount must be greater than zero.",
                };
            }
        }

        return errors;
    }
}
