using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using BackendVsaOwin.Modules.Orders.Features.ListOrders;
using NSwag.Annotations;

namespace BackendVsaOwin.Modules.Orders.Features;

public sealed partial class OrdersController
{
    /// <summary>
    /// Returns all orders currently available to the module.
    /// </summary>
    [HttpGet]
    [Route("")]
    [SwaggerResponse(
        HttpStatusCode.OK,
        typeof(ListOrdersResponse),
        Description = "Orders returned.")]
    public async Task<IHttpActionResult> List(CancellationToken cancellationToken)
    {
        var response = await _listHandler.HandleAsync(cancellationToken);

        return Ok(response);
    }
}
