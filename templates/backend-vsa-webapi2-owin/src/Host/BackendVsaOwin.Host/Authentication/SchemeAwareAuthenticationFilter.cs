using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Filters;
using Microsoft.Owin.Security.OAuth;

namespace BackendVsaOwin.Host.Authentication;

/// <summary>
/// Challenges the authentication scheme selected by the incoming credentials.
/// </summary>
internal sealed class SchemeAwareAuthenticationFilter : Attribute, IAuthenticationFilter
{
    public bool AllowMultiple => false;

    public Task AuthenticateAsync(
        HttpAuthenticationContext context,
        CancellationToken cancellationToken)
    {
        // Katana authentication middleware has already populated the request principal.
        return Task.CompletedTask;
    }

    public Task ChallengeAsync(
        HttpAuthenticationChallengeContext context,
        CancellationToken cancellationToken)
    {
        context.Result = new SchemeAwareChallengeResult(
            context.Result,
            context.Request);
        return Task.CompletedTask;
    }

    private sealed class SchemeAwareChallengeResult : IHttpActionResult
    {
        private readonly IHttpActionResult _innerResult;
        private readonly HttpRequestMessage _request;

        public SchemeAwareChallengeResult(
            IHttpActionResult innerResult,
            HttpRequestMessage request)
        {
            _innerResult = innerResult
                ?? throw new ArgumentNullException(nameof(innerResult));
            _request = request
                ?? throw new ArgumentNullException(nameof(request));
        }

        public async Task<HttpResponseMessage> ExecuteAsync(
            CancellationToken cancellationToken)
        {
            var response = await _innerResult
                .ExecuteAsync(cancellationToken)
                .ConfigureAwait(false);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                ChallengeSelectedScheme();
            }

            return response;
        }

        private void ChallengeSelectedScheme()
        {
            var scheme = _request.Headers.Authorization?.Scheme;
            var owinContext = _request.GetOwinContext();
            var authentication = owinContext.Authentication;

            if (string.Equals(
                    scheme,
                    BasicAuthenticationOptions.Scheme,
                    StringComparison.OrdinalIgnoreCase))
            {
                authentication.Challenge(BasicAuthenticationOptions.Scheme);
                return;
            }

            if (string.Equals(
                    scheme,
                    OAuthDefaults.AuthenticationType,
                    StringComparison.OrdinalIgnoreCase)
                || !string.IsNullOrEmpty(
                    owinContext.Request.Query[OAuthParameters.AccessToken]))
            {
                authentication.Challenge(OAuthDefaults.AuthenticationType);
                return;
            }

            authentication.Challenge(
                BasicAuthenticationOptions.Scheme,
                OAuthDefaults.AuthenticationType);
        }
    }
}
