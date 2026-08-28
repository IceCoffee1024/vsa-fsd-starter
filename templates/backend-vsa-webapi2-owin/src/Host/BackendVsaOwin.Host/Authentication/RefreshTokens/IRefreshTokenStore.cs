using System;
using System.Threading;
using System.Threading.Tasks;

namespace BackendVsaOwin.Host.Authentication.RefreshTokens;

internal interface IRefreshTokenStore
{
    Task<bool> StoreAsync(
        RefreshTokenEntry entry,
        CancellationToken cancellationToken);

    Task<string?> RedeemAsync(
        string tokenHash,
        DateTimeOffset redeemedUtc,
        CancellationToken cancellationToken);
}

internal sealed class RefreshTokenEntry
{
    public RefreshTokenEntry(
        string tokenHash,
        string familyId,
        string clientId,
        string protectedTicket,
        DateTimeOffset createdUtc,
        DateTimeOffset expiresUtc)
    {
        TokenHash = tokenHash;
        FamilyId = familyId;
        ClientId = clientId;
        ProtectedTicket = protectedTicket;
        CreatedUtc = createdUtc;
        ExpiresUtc = expiresUtc;
    }

    public string TokenHash { get; }

    public string FamilyId { get; }

    public string ClientId { get; }

    public string ProtectedTicket { get; }

    public DateTimeOffset CreatedUtc { get; }

    public DateTimeOffset ExpiresUtc { get; }
}
