using System;
using System.Threading.Tasks;
using HostApplicationIdentity = BackendVsaOwin.Host.Composition.ApplicationIdentity;
using Microsoft.Owin;
using Microsoft.Owin.Security.OAuth;

namespace BackendVsaOwin.Host.Authentication;

internal static class OAuthOptionsFactory
{
    public static OAuthAuthorizationServerOptions CreateAuthorizationServer(
        ICredentialValidator credentialValidator)
    {
        if (credentialValidator is null)
        {
            throw new ArgumentNullException(nameof(credentialValidator));
        }

        return new OAuthAuthorizationServerOptions
        {
            AuthenticationType = OAuthDefaults.AuthenticationType,
            TokenEndpointPath = new PathString(OAuthEndpoints.TokenPath),
            AllowInsecureHttp = true,
            AccessTokenExpireTimeSpan = TimeSpan.FromHours(1),
            Provider = new PasswordGrantOAuthProvider(credentialValidator),
        };
    }

    public static OAuthBearerAuthenticationOptions CreateBearer()
    {
        return new OAuthBearerAuthenticationOptions
        {
            AuthenticationType = OAuthDefaults.AuthenticationType,
            Realm = HostApplicationIdentity.ApplicationName,
            Provider = new OAuthBearerAuthenticationProvider
            {
                OnRequestToken = context =>
                {
                    if (string.IsNullOrEmpty(context.Token))
                    {
                        context.Token =
                            context.OwinContext.Request.Query[
                                OAuthParameters.AccessToken];
                    }

                    return Task.CompletedTask;
                },
            },
        };
    }
}
