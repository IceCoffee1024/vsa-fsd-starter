using System.Collections.Generic;

namespace BackendVsaOwin.Modules.Orders.Features.UpdateOrder;

public sealed class UpdateOrderValidator
{
    public IReadOnlyDictionary<string, string[]> Validate(UpdateOrderRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (request.TotalAmount <= 0)
        {
            errors["totalAmount"] = new[] { "Total amount must be greater than zero." };
        }

        return errors;
    }
}
