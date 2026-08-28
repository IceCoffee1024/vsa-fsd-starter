using System;
using System.Threading.Tasks;
using BackendVsaOwin.Host.Authentication.RefreshTokens;
using HostApplicationIdentity = BackendVsaOwin.Host.Composition.ApplicationIdentity;
using Microsoft.Owin;
using Microsoft.Owin.Security.OAuth;

namespace BackendVsaOwin.Host.Authentication;

internal static class OAuthOptionsFactory
{
    public static OAuthAuthorizationServerOptions CreateAuthorizationServer(
        ICredentialValidator credentialValidator,
        IRefreshTokenStore refreshTokenStore)
    {
        if (credentialValidator is null)
        {
            throw new ArgumentNullException(nameof(credentialValidator));
        }

        if (refreshTokenStore is null)
        {
            throw new ArgumentNullException(nameof(refreshTokenStore));
        }

        return new OAuthAuthorizationServerOptions
        {
            AuthenticationType = OAuthDefaults.AuthenticationType,
            TokenEndpointPath = new PathString(OAuthEndpoints.TokenPath),
            AllowInsecureHttp = true,
            AccessTokenExpireTimeSpan = TimeSpan.FromHours(1),
            Provider = new OAuthAuthorizationProvider(credentialValidator),
            RefreshTokenProvider = new RotatingRefreshTokenProvider(refreshTokenStore),
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
