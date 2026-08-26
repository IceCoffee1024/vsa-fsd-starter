using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using BackendVsaOwin.Modules.Customers.Features.CreateCustomer;
using BackendVsaOwin.BuildingBlocks.WebApi.Errors;
using NSwag.Annotations;

namespace BackendVsaOwin.Modules.Customers.Features;

public sealed partial class CustomersController
{
    /// <summary>
    /// Creates a customer after validating the display name.
    /// </summary>
    [HttpPost]
    [Route("")]
    [SwaggerResponse(
        HttpStatusCode.Created,
        typeof(CreateCustomerResponse),
        Description = "Customer created.")]
    [SwaggerResponse(
        HttpStatusCode.BadRequest,
        typeof(ValidationProblemDetailsResponse),
        Description = "Request validation failed.")]
    public async Task<IHttpActionResult> Create(
        CreateCustomerRequest? request,
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

        var response = await _createHandler
            .HandleAsync(request, cancellationToken)
            .ConfigureAwait(false);

        return CreatedAtRoute(
            "GetCustomerById",
            new { id = response.Id },
            response);
    }
}
