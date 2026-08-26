using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using BackendVsaOwin.BuildingBlocks.WebApi.Tracing;

namespace BackendVsaOwin.BuildingBlocks.WebApi.Errors;

/// <summary>
/// Produces Web API 2 action results with the RFC 9457 media type.
/// </summary>
public static class ProblemDetailsResults
{
    private const string ProblemJsonMediaType = "application/problem+json";

    /// <summary>
    /// Creates an action result for a known problem while preserving the supplied HTTP status.
    /// </summary>
    public static IHttpActionResult Problem(
        this ApiController controller,
        HttpStatusCode statusCode,
        string type,
        string title,
        string detail)
    {
        if (controller is null)
        {
            throw new ArgumentNullException(nameof(controller));
        }

        var problem = ProblemDetailsFactory.Create(
            controller.Request,
            statusCode,
            type,
            title,
            detail);

        return Create(controller.Request, problem);
    }

    /// <summary>
    /// Creates an HTTP 400 action result containing field-level validation messages.
    /// </summary>
    public static IHttpActionResult ValidationProblem(
        this ApiController controller,
        IReadOnlyDictionary<string, string[]> errors)
    {
        if (controller is null)
        {
            throw new ArgumentNullException(nameof(controller));
        }

        var problem = ProblemDetailsFactory.CreateValidation(
            controller.Request,
            errors);

        return Create(controller.Request, problem);
    }

    /// <summary>
    /// Creates a validation problem for an action whose required JSON body was absent.
    /// </summary>
    public static IHttpActionResult MissingRequestBody(
        this ApiController controller)
    {
        if (controller is null)
        {
            throw new ArgumentNullException(nameof(controller));
        }

        return controller.ValidationProblem(
            new Dictionary<string, string[]>
            {
                ["request"] = new[] { "A JSON request body is required." },
            });
    }

    /// <summary>
    /// Creates an action result for use outside an <see cref="ApiController" />, such as a global exception handler.
    /// </summary>
    public static IHttpActionResult Create(
        HttpRequestMessage request,
        ProblemDetailsResponse problem)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (problem is null)
        {
            throw new ArgumentNullException(nameof(problem));
        }

        return new ProblemDetailsResult(request, problem);
    }

    private sealed class ProblemDetailsResult : IHttpActionResult
    {
        private readonly ProblemDetailsResponse _problem;
        private readonly HttpRequestMessage _request;

        public ProblemDetailsResult(
            HttpRequestMessage request,
            ProblemDetailsResponse problem)
        {
            _request = request;
            _problem = problem;
        }

        public Task<HttpResponseMessage> ExecuteAsync(
            CancellationToken cancellationToken)
        {
            var response = _request.CreateResponse(
                (HttpStatusCode)_problem.Status,
                _problem);
            response.Content.Headers.ContentType =
                new MediaTypeHeaderValue(ProblemJsonMediaType);
            response.Headers.TryAddWithoutValidation(
                RequestTraceHandler.TraceIdHeaderName,
                _problem.TraceId);

            return Task.FromResult(response);
        }
    }
}
