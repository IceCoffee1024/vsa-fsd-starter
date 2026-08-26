using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using BackendVsaOwin.Modules.Orders.Features.GetOrder;
using BackendVsaOwin.BuildingBlocks.WebApi.Errors;
using NSwag.Annotations;

namespace BackendVsaOwin.Modules.Orders.Features;

public sealed partial class OrdersController
{
    /// <summary>
    /// Returns the order with the specified identifier, or a not-found error.
    /// </summary>
    [HttpGet]
    [Route("{id:guid}", Name = "GetOrderById")]
    [SwaggerResponse(
        HttpStatusCode.OK,
        typeof(GetOrderResponse),
        Description = "Order returned.")]
    [SwaggerResponse(
        HttpStatusCode.NotFound,
        typeof(ProblemDetailsResponse),
        Description = "Order not found.")]
    public async Task<IHttpActionResult> Get(
        Guid id,
        CancellationToken cancellationToken)
    {
        var response = await _getHandler.HandleAsync(id, cancellationToken);

        return response is null
            ? OrderNotFound(id)
            : Ok(response);
    }
}
