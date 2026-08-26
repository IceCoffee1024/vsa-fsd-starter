using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BackendVsaOwin.Modules.Customers.Contracts;

/// <summary>
/// Public Customers module contract for resolving customer identities across module boundaries.
/// </summary>
public interface ICustomerLookup
{
    /// <summary>
    /// Resolves a customer without exposing Customers persistence; returns null when the ID is unknown.
    /// </summary>
    Task<CustomerReference?> FindAsync(
        Guid customerId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Resolves the existing customers from a batch without exposing Customers persistence.
    /// Missing identifiers are omitted from the returned dictionary.
    /// </summary>
    Task<IReadOnlyDictionary<Guid, CustomerReference>> FindManyAsync(
        IReadOnlyCollection<Guid> customerIds,
        CancellationToken cancellationToken);
}
