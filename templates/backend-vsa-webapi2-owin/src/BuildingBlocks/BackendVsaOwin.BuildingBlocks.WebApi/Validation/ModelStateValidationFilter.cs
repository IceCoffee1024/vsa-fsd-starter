using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using BackendVsaOwin.BuildingBlocks.WebApi.Errors;

namespace BackendVsaOwin.BuildingBlocks.WebApi.Validation;

/// <summary>
/// Rejects transport-level binding errors before a controller action runs.
/// </summary>
public sealed class ModelStateValidationFilter : ActionFilterAttribute
{
    /// <summary>
    /// Converts invalid Web API ModelState into the shared validation problem response.
    /// </summary>
    public override async Task OnActionExecutingAsync(
        HttpActionContext actionContext,
        CancellationToken cancellationToken)
    {
        if (actionContext is null)
        {
            throw new ArgumentNullException(nameof(actionContext));
        }

        if (actionContext.ModelState.IsValid)
        {
            return;
        }

        var errors = ModelStateErrorMapper.ToErrors(actionContext.ModelState);
        var problem = ProblemDetailsFactory.CreateValidation(
            actionContext.Request,
            errors);

        actionContext.Response = await ProblemDetailsResults
            .Create(actionContext.Request, problem)
            .ExecuteAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
