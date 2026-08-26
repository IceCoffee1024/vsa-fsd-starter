using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using BackendVsaOwin.Modules.Orders.Features.DeleteOrder;
using BackendVsaOwin.BuildingBlocks.WebApi.Errors;
using NSwag.Annotations;

namespace BackendVsaOwin.Modules.Orders.Features;

public sealed partial class OrdersController
{
    /// <summary>
    /// Deletes an order and returns no content when successful.
    /// </summary>
    [HttpDelete]
    [Route("{id:guid}")]
    [SwaggerResponse(
        HttpStatusCode.NoContent,
        typeof(void),
        Description = "Order deleted.")]
    [SwaggerResponse(
        HttpStatusCode.NotFound,
        typeof(ProblemDetailsResponse),
        Description = "Order not found.")]
    public async Task<IHttpActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var deleted = await _deleteHandler
            .HandleAsync(id, cancellationToken)
            .ConfigureAwait(false);

        return deleted
            ? StatusCode(HttpStatusCode.NoContent)
            : OrderNotFound(id);
    }
}
