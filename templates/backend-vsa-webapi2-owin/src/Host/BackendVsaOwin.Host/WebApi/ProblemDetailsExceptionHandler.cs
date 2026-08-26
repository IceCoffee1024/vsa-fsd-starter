using System.Web.Http.ExceptionHandling;
using BackendVsaOwin.BuildingBlocks.WebApi.Errors;
using BackendVsaOwin.BuildingBlocks.WebApi.Tracing;
using Microsoft.Extensions.Logging;

namespace BackendVsaOwin.Host.WebApi;

internal sealed class ProblemDetailsExceptionHandler : ExceptionHandler
{
    private readonly ILogger<ProblemDetailsExceptionHandler> _logger;

    public ProblemDetailsExceptionHandler(
        ILogger<ProblemDetailsExceptionHandler> logger)
    {
        _logger = logger;
    }

    public override void Handle(ExceptionHandlerContext context)
    {
        var traceId = RequestTraceContext.GetTraceId(context.Request);
        _logger.LogError(
            context.Exception,
            "Unhandled exception while processing {HttpMethod} {RequestPath}. TraceId: {TraceId}",
            context.Request.Method.Method,
            context.Request.RequestUri?.PathAndQuery,
            traceId);

        var problem = ProblemDetailsFactory.CreateInternalServerError(
            context.Request);

        context.Result = ProblemDetailsResults.Create(
            context.Request,
            problem);
    }
}
