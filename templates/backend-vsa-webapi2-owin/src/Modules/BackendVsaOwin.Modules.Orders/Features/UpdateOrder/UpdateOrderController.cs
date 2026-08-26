using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using BackendVsaOwin.Modules.Orders.Features.UpdateOrder;
using BackendVsaOwin.BuildingBlocks.WebApi.Errors;
using NSwag.Annotations;

namespace BackendVsaOwin.Modules.Orders.Features;

public sealed partial class OrdersController
{
    /// <summary>
    /// Updates an existing order after validating the request.
    /// </summary>
    [HttpPut]
    [Route("{id:guid}")]
    [SwaggerResponse(
        HttpStatusCode.OK,
        typeof(UpdateOrderResponse),
        Description = "Order updated.")]
    [SwaggerResponse(
        HttpStatusCode.BadRequest,
        typeof(ValidationProblemDetailsResponse),
        Description = "Request validation failed.")]
    [SwaggerResponse(
        HttpStatusCode.NotFound,
        typeof(ProblemDetailsResponse),
        Description = "Order not found.")]
    public async Task<IHttpActionResult> Update(
        Guid id,
        UpdateOrderRequest? request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return this.MissingRequestBody();
        }

        var errors = _updateValidator.Validate(request);
        if (errors.Count > 0)
        {
            return this.ValidationProblem(errors);
        }

        var response = await _updateHandler.HandleAsync(id, request, cancellationToken);

        return response is null
            ? OrderNotFound(id)
            : Ok(response);
    }
}
