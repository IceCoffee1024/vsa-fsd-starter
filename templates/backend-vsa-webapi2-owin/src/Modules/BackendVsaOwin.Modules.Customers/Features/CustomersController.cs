using System;
using System.Net;
using System.Web.Http;
using BackendVsaOwin.Modules.Customers.Features.CreateCustomer;
using BackendVsaOwin.Modules.Customers.Features.GetCustomer;
using BackendVsaOwin.BuildingBlocks.WebApi.Errors;

namespace BackendVsaOwin.Modules.Customers.Features;

/// <summary>
/// Customer management endpoints for creating and querying customers.
/// </summary>
[RoutePrefix("api/customers")]
public sealed partial class CustomersController : ApiController
{
    private const string CustomerNotFoundProblemType =
        ProblemTypeUris.Prefix + ":customer-not-found";

    private readonly CreateCustomerHandler _createHandler;
    private readonly CreateCustomerValidator _createValidator;
    private readonly GetCustomerHandler _getHandler;

    public CustomersController(
        CreateCustomerValidator createValidator,
        CreateCustomerHandler createHandler,
        GetCustomerHandler getHandler)
    {
        _createValidator = createValidator;
        _createHandler = createHandler;
        _getHandler = getHandler;
    }

    private IHttpActionResult CustomerNotFound(Guid id)
    {
        return this.Problem(
            HttpStatusCode.NotFound,
            CustomerNotFoundProblemType,
            "Customer not found",
            $"No customer with identifier '{id}' exists.");
    }
}
