using System;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Infrastructure;

namespace BackendVsaOwin.Host.Authentication;

/// <summary>
/// Authenticates one request and applies the Basic challenge to unauthorized responses.
/// </summary>
internal sealed class BasicAuthenticationHandler
    : AuthenticationHandler<BasicAuthenticationOptions>
{
    private const string AuthorizationHeaderName = "Authorization";

    private readonly ICredentialValidator _credentialValidator;

    public BasicAuthenticationHandler(ICredentialValidator credentialValidator)
    {
        _credentialValidator = credentialValidator
            ?? throw new ArgumentNullException(nameof(credentialValidator));
    }

    protected override Task<AuthenticationTicket> AuthenticateCoreAsync()
    {
        if (!TryReadCredentials(
                Request.Headers[AuthorizationHeaderName],
                out var username,
                out var password)
            || !_credentialValidator.Validate(username, password))
        {
            return Task.FromResult<AuthenticationTicket>(null!);
        }

        var identity = new ClaimsIdentity(Options.AuthenticationType);
        identity.AddClaim(new Claim(ClaimTypes.Name, username));
        var ticket = new AuthenticationTicket(
            identity,
            new AuthenticationProperties());

        return Task.FromResult(ticket);
    }

    protected override Task ApplyResponseChallengeAsync()
    {
        if (Response.StatusCode == 401
            && Helper.LookupChallenge(
                Options.AuthenticationType,
                Options.AuthenticationMode) is not null)
        {
            Response.Headers.Append(
                "WWW-Authenticate",
                $"{BasicAuthenticationOptions.Scheme} realm=\"{Options.Realm}\"");
        }

        return Task.CompletedTask;
    }

    private static bool TryReadCredentials(
        string? authorizationHeader,
        out string username,
        out string password)
    {
        username = string.Empty;
        password = string.Empty;

        if (string.IsNullOrWhiteSpace(authorizationHeader))
        {
            return false;
        }

        var separator = authorizationHeader!.IndexOf(' ');
        if (separator <= 0
            || !string.Equals(
                authorizationHeader[..separator],
                BasicAuthenticationOptions.Scheme,
                StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        try
        {
            var encodedCredentials = authorizationHeader![(separator + 1)..].Trim();
            var decodedCredentials = Encoding.UTF8.GetString(
                Convert.FromBase64String(encodedCredentials));
            var credentialSeparator = decodedCredentials.IndexOf(':');
            if (credentialSeparator <= 0)
            {
                return false;
            }

            username = decodedCredentials[..credentialSeparator];
            password = decodedCredentials[(credentialSeparator + 1)..];
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
