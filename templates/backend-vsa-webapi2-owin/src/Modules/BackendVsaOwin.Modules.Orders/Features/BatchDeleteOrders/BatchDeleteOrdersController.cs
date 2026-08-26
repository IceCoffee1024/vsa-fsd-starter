using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using BackendVsaOwin.Modules.Orders.Features.BatchDeleteOrders;
using BackendVsaOwin.BuildingBlocks.WebApi.Errors;
using NSwag.Annotations;

namespace BackendVsaOwin.Modules.Orders.Features;

public sealed partial class OrdersController
{
    /// <summary>
    /// Deletes up to 100 unique orders and reports identifiers that were not found.
    /// </summary>
    [HttpPost]
    [Route("batch-delete")]
    [SwaggerResponse(
        HttpStatusCode.OK,
        typeof(BatchDeleteOrdersResponse),
        Description = "Batch deletion result.")]
    [SwaggerResponse(
        HttpStatusCode.BadRequest,
        typeof(ValidationProblemDetailsResponse),
        Description = "Request validation failed.")]
    public async Task<IHttpActionResult> BatchDelete(
        BatchDeleteOrdersRequest? request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return this.MissingRequestBody();
        }

        var errors = _batchDeleteValidator.Validate(request);
        if (errors.Count > 0)
        {
            return this.ValidationProblem(errors);
        }

        var response = await _batchDeleteHandler.HandleAsync(request, cancellationToken);

        return Ok(response);
    }
}
