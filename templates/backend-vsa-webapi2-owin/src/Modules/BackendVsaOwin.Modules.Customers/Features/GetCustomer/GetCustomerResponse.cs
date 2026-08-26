using System;

namespace BackendVsaOwin.Modules.Customers.Features.GetCustomer;

public sealed class GetCustomerResponse
{
    public required Guid Id { get; init; }

    public required string DisplayName { get; init; }
}
