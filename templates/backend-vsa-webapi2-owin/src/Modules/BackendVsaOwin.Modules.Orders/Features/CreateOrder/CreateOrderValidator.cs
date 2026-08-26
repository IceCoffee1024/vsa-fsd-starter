using System;
using System.Collections.Generic;

namespace BackendVsaOwin.Modules.Orders.Features.CreateOrder;

public sealed class CreateOrderValidator
{
    public IReadOnlyDictionary<string, string[]> Validate(CreateOrderRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (request.CustomerId == Guid.Empty)
        {
            errors["customerId"] = new[] { "Customer ID is required." };
        }

        if (request.TotalAmount <= 0)
        {
            errors["totalAmount"] = new[] { "Total amount must be greater than zero." };
        }

        return errors;
    }
}
