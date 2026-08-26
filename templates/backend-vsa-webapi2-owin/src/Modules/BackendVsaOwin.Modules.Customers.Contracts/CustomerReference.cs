using System;

namespace BackendVsaOwin.Modules.Customers.Contracts;

/// <summary>
/// Stable customer data that other modules may use without depending on Customers internals.
/// </summary>
public sealed class CustomerReference
{
    public CustomerReference(Guid id, string displayName)
    {
        Id = id;
        DisplayName = displayName;
    }

    public Guid Id { get; }

    /// <summary>
    /// Current customer name; consumers decide whether to retain it as a historical snapshot.
    /// </summary>
    public string DisplayName { get; }
}
