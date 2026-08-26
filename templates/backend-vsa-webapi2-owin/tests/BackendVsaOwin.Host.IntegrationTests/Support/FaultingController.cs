using System;
using System.Web.Http;

namespace BackendVsaOwin.Host.IntegrationTests.Support;

[RoutePrefix("__integration-tests/fault")]
public sealed class FaultingController : ApiController
{
    [HttpGet]
    [Route("")]
    public IHttpActionResult Get()
    {
        throw new InvalidOperationException("Sensitive diagnostic detail.");
    }
}
