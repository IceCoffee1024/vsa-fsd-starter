using System;

namespace BackendVsaOwin.Modules.Customers.Features.CreateCustomer;

public sealed class CreateCustomerResponse
{
    public required Guid Id { get; init; }

    public required string DisplayName { get; init; }
}
