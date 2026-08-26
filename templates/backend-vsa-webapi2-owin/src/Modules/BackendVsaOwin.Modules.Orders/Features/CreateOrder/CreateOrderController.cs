using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using BackendVsaOwin.Modules.Orders.Features.CreateOrder;
using BackendVsaOwin.BuildingBlocks.WebApi.Errors;
using NSwag.Annotations;

namespace BackendVsaOwin.Modules.Orders.Features;

public sealed partial class OrdersController
{
    /// <summary>
    /// Creates an order after validating the request.
    /// </summary>
    [HttpPost]
    [Route("")]
    [SwaggerResponse(
        HttpStatusCode.Created,
        typeof(CreateOrderResponse),
        Description = "Order created.")]
    [SwaggerResponse(
        HttpStatusCode.BadRequest,
        typeof(ValidationProblemDetailsResponse),
        Description = "Request validation failed.")]
    public async Task<IHttpActionResult> Create(
        CreateOrderRequest? request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return this.MissingRequestBody();
        }

        var errors = _createValidator.Validate(request);
        if (errors.Count > 0)
        {
            return this.ValidationProblem(errors);
        }

        var result = await _createHandler
            .HandleAsync(request, cancellationToken)
            .ConfigureAwait(false);
        if (result.Response is null)
        {
            return this.ValidationProblem(result.Errors);
        }

        return CreatedAtRoute(
            "GetOrderById",
            new { id = result.Response.Id },
            result.Response);
    }
}
