using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin.Security.Infrastructure;

namespace BackendVsaOwin.Host.Authentication.RefreshTokens;

internal sealed class RotatingRefreshTokenProvider : AuthenticationTokenProvider
{
    internal static readonly TimeSpan Lifetime = TimeSpan.FromDays(30);

    private readonly IRefreshTokenStore _store;

    public RotatingRefreshTokenProvider(IRefreshTokenStore store)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
    }

    public override async Task CreateAsync(AuthenticationTokenCreateContext context)
    {
        var issuedUtc = DateTimeOffset.UtcNow;
        var expiresUtc = issuedUtc.Add(Lifetime);
        var properties = context.Ticket.Properties;
        if (!properties.Dictionary.TryGetValue(
                OAuthTicketProperties.RefreshTokenFamilyId,
                out var familyId)
            || string.IsNullOrWhiteSpace(familyId))
        {
            familyId = Guid.NewGuid().ToString("N");
            properties.Dictionary[OAuthTicketProperties.RefreshTokenFamilyId] = familyId;
        }

        properties.IssuedUtc = issuedUtc;
        properties.ExpiresUtc = expiresUtc;
        properties.Dictionary.TryGetValue(
            OAuthTicketProperties.ClientId,
            out var clientId);

        var token = GenerateToken();
        var entry = new RefreshTokenEntry(
            HashToken(token),
            familyId,
            clientId ?? string.Empty,
            context.SerializeTicket(),
            issuedUtc,
            expiresUtc);
        if (await _store.StoreAsync(entry, context.Request.CallCancelled))
        {
            context.SetToken(token);
        }
    }

    public override async Task ReceiveAsync(AuthenticationTokenReceiveContext context)
    {
        if (string.IsNullOrWhiteSpace(context.Token))
        {
            return;
        }

        var protectedTicket = await _store.RedeemAsync(
            HashToken(context.Token),
            DateTimeOffset.UtcNow,
            context.Request.CallCancelled);
        if (protectedTicket is not null)
        {
            context.DeserializeTicket(protectedTicket);
        }
    }

    private static string GenerateToken()
    {
        var bytes = new byte[32];
        using var random = RandomNumberGenerator.Create();
        random.GetBytes(bytes);
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static string HashToken(string token)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
        return BitConverter.ToString(hash)
            .Replace("-", string.Empty)
            .ToLowerInvariant();
    }
}
