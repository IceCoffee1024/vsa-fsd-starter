using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using BackendVsaOwin.Modules.Customers.Features.GetCustomer;
using BackendVsaOwin.BuildingBlocks.WebApi.Errors;
using NSwag.Annotations;

namespace BackendVsaOwin.Modules.Customers.Features;

public sealed partial class CustomersController
{
    /// <summary>
    /// Returns the customer with the specified identifier, or a not-found error.
    /// </summary>
    [HttpGet]
    [Route("{id:guid}", Name = "GetCustomerById")]
    [SwaggerResponse(
        HttpStatusCode.OK,
        typeof(GetCustomerResponse),
        Description = "Customer returned.")]
    [SwaggerResponse(
        HttpStatusCode.NotFound,
        typeof(ProblemDetailsResponse),
        Description = "Customer not found.")]
    public async Task<IHttpActionResult> Get(
        Guid id,
        CancellationToken cancellationToken)
    {
        var response = await _getHandler.HandleAsync(id, cancellationToken);

        return response is null
            ? CustomerNotFound(id)
            : Ok(response);
    }
}
