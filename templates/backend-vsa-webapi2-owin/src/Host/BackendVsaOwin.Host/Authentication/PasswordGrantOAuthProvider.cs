using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Owin.Security.OAuth;

namespace BackendVsaOwin.Host.Authentication;

/// <summary>
/// Issues a demo bearer token after validating the configured resource-owner credentials.
/// </summary>
internal sealed class PasswordGrantOAuthProvider : OAuthAuthorizationServerProvider
{
    private readonly ICredentialValidator _credentialValidator;

    public PasswordGrantOAuthProvider(ICredentialValidator credentialValidator)
    {
        _credentialValidator = credentialValidator
            ?? throw new ArgumentNullException(nameof(credentialValidator));
    }

    public override Task ValidateClientAuthentication(
        OAuthValidateClientAuthenticationContext context)
    {
        // This starter uses a public password-grant client. The resource-owner credentials
        // remain the required authentication factor in GrantResourceOwnerCredentials.
        context.Validated();
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
        context.Validated(identity);
        return Task.CompletedTask;
    }
}
