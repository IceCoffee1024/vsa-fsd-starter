using System;
using System.Collections.Generic;
using System.Linq;

namespace BackendVsaOwin.Modules.Orders.Features.BatchDeleteOrders;

public sealed class BatchDeleteOrdersValidator
{
    private const int MaximumBatchSize = 100;

    public IReadOnlyDictionary<string, string[]> Validate(
        BatchDeleteOrdersRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (request.Ids is null || request.Ids.Length == 0)
        {
            errors["ids"] = new[] { "At least one order ID is required." };
            return errors;
        }

        var idErrors = new List<string>();

        if (request.Ids.Length > MaximumBatchSize)
        {
            idErrors.Add($"A maximum of {MaximumBatchSize} order IDs is allowed.");
        }

        if (request.Ids.Contains(Guid.Empty))
        {
            idErrors.Add("Order IDs must not be empty GUIDs.");
        }

        if (request.Ids.Distinct().Count() != request.Ids.Length)
        {
            idErrors.Add("Order IDs must be unique.");
        }

        if (idErrors.Count > 0)
        {
            errors["ids"] = idErrors.ToArray();
        }

        return errors;
    }
}
