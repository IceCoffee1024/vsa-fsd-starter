using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using BackendVsaOwin.Modules.Orders.Features.BatchCreateOrders;
using BackendVsaOwin.BuildingBlocks.WebApi.Errors;
using NSwag.Annotations;

namespace BackendVsaOwin.Modules.Orders.Features;

public sealed partial class OrdersController
{
    /// <summary>
    /// Creates between 1 and 100 orders after validating every item.
    /// </summary>
    [HttpPost]
    [Route("batch")]
    [SwaggerResponse(
        HttpStatusCode.Created,
        typeof(BatchCreateOrdersResponse),
        Description = "Orders created.")]
    [SwaggerResponse(
        HttpStatusCode.BadRequest,
        typeof(ValidationProblemDetailsResponse),
        Description = "Request validation failed.")]
    public async Task<IHttpActionResult> BatchCreate(
        BatchCreateOrdersRequest? request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return this.MissingRequestBody();
        }

        var errors = _batchCreateValidator.Validate(request);
        if (errors.Count > 0)
        {
            return this.ValidationProblem(errors);
        }

        var result = await _batchCreateHandler.HandleAsync(request, cancellationToken);
        if (result.Response is null)
        {
            return this.ValidationProblem(result.Errors);
        }

        return Content(HttpStatusCode.Created, result.Response);
    }
}
