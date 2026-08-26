using System.Web.Http;

namespace BackendVsaOwin.Host.IntegrationTests.Support;

[RoutePrefix("__integration-tests/authentication")]
public sealed class AuthenticationProbeController : ApiController
{
    [HttpGet]
    [Route("")]
    public IHttpActionResult Get()
    {
        var identity = User.Identity;

        return Ok(new
        {
            identity.Name,
            identity.AuthenticationType,
            identity.IsAuthenticated,
        });
    }
}
