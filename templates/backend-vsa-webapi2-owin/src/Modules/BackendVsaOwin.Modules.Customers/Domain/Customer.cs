using System;

namespace BackendVsaOwin.Modules.Customers.Domain;

internal sealed class Customer
{
    public Customer(Guid id, string displayName)
    {
        Id = id;
        DisplayName = displayName;
    }

    public Guid Id { get; }

    public string DisplayName { get; }
}
