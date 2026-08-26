using System;
using Owin;

namespace BackendVsaOwin.Host.Authentication;

internal static class BasicAuthenticationAppBuilderExtensions
{
    public static IAppBuilder UseBasicAuthentication(
        this IAppBuilder app,
        BasicAuthenticationOptions options,
        ICredentialValidator credentialValidator)
    {
        if (app is null)
        {
            throw new ArgumentNullException(nameof(app));
        }

        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        if (credentialValidator is null)
        {
            throw new ArgumentNullException(nameof(credentialValidator));
        }

        return app.Use<BasicAuthenticationMiddleware>(
            options,
            credentialValidator);
    }
}
