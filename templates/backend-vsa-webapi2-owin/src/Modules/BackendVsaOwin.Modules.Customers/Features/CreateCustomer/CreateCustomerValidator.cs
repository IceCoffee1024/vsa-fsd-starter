using System.Collections.Generic;

namespace BackendVsaOwin.Modules.Customers.Features.CreateCustomer;

public sealed class CreateCustomerValidator
{
    public IReadOnlyDictionary<string, string[]> Validate(
        CreateCustomerRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.DisplayName))
        {
            errors["displayName"] = new[] { "Display name is required." };
        }
        else if (request.DisplayName!.Trim().Length > 100)
        {
            errors["displayName"] = new[]
            {
                "Display name must not exceed 100 characters.",
            };
        }

        return errors;
    }
}
