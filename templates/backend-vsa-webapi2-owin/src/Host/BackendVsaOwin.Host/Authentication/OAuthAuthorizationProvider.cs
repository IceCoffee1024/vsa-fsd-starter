using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;

namespace BackendVsaOwin.Host.Authentication;

/// <summary>
/// Validates the demo password and refresh-token grants for public clients.
/// </summary>
internal sealed class OAuthAuthorizationProvider : OAuthAuthorizationServerProvider
{
    private readonly ICredentialValidator _credentialValidator;

    public OAuthAuthorizationProvider(ICredentialValidator credentialValidator)
    {
        _credentialValidator = credentialValidator
            ?? throw new ArgumentNullException(nameof(credentialValidator));
    }

    public override Task ValidateClientAuthentication(
        OAuthValidateClientAuthenticationContext context)
    {
        // This starter uses a public password-grant client. The resource-owner credentials
        // remain the required authentication factor in GrantResourceOwnerCredentials.
        var clientId = context.Parameters.Get(OAuthParameters.ClientId);
        if (string.IsNullOrWhiteSpace(clientId))
        {
            context.Validated();
        }
        else
        {
            context.Validated(clientId);
        }

        return Task.CompletedTask;
    }

    public override Task GrantResourceOwnerCredentials(
        OAuthGrantResourceOwnerCredentialsContext context)
    {
        if (!_credentialValidator.Validate(context.UserName, context.Password))
        {
            context.SetError("invalid_grant", "The resource-owner credentials are invalid.");
            return Task.CompletedTask;
        }

        var identity = new ClaimsIdentity(
            OAuthDefaults.AuthenticationType,
            ClaimsIdentity.DefaultNameClaimType,
            ClaimsIdentity.DefaultRoleClaimType);
        identity.AddClaim(new Claim(ClaimTypes.Name, context.UserName));
        var properties = new AuthenticationProperties();
        properties.Dictionary[OAuthTicketProperties.ClientId] =
            context.ClientId ?? string.Empty;
        context.Validated(new AuthenticationTicket(identity, properties));
        return Task.CompletedTask;
    }

    public override Task GrantRefreshToken(OAuthGrantRefreshTokenContext context)
    {
        context.Ticket.Properties.Dictionary.TryGetValue(
            OAuthTicketProperties.ClientId,
            out var originalClientId);

        if (!string.Equals(
                originalClientId ?? string.Empty,
                context.ClientId ?? string.Empty,
                StringComparison.Ordinal))
        {
            context.SetError(
                "invalid_grant",
                "The refresh token was not issued to this client.");
            return Task.CompletedTask;
        }

        context.Validated(context.Ticket);
        return Task.CompletedTask;
    }
}
