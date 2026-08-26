using System;
using Microsoft.Owin;
using Microsoft.Owin.Security.Infrastructure;

namespace BackendVsaOwin.Host.Authentication;

/// <summary>
/// Connects the Basic authentication scheme to its per-request handler.
/// </summary>
internal sealed class BasicAuthenticationMiddleware
    : AuthenticationMiddleware<BasicAuthenticationOptions>
{
    private readonly ICredentialValidator _credentialValidator;

    public BasicAuthenticationMiddleware(
        OwinMiddleware next,
        BasicAuthenticationOptions options,
        ICredentialValidator credentialValidator)
        : base(next, options)
    {
        _credentialValidator = credentialValidator
            ?? throw new ArgumentNullException(nameof(credentialValidator));
    }

    protected override AuthenticationHandler<BasicAuthenticationOptions>
        CreateHandler()
    {
        return new BasicAuthenticationHandler(_credentialValidator);
    }
}
