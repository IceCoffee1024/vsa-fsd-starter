using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using BackendVsaOwin.BuildingBlocks.WebApi.Tracing;

namespace BackendVsaOwin.BuildingBlocks.WebApi.Errors;

/// <summary>
/// Creates consistent RFC 9457 response bodies while deriving the occurrence URI from the request.
/// </summary>
public static class ProblemDetailsFactory
{
    /// <summary>
    /// Creates a problem for a known API error.
    /// </summary>
    public static ProblemDetailsResponse Create(
        HttpRequestMessage request,
        HttpStatusCode statusCode,
        string type,
        string title,
        string detail)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        return new ProblemDetailsResponse
        {
            Type = type,
            Title = title,
            Status = (int)statusCode,
            Detail = detail,
            Instance = GetInstance(request),
            TraceId = RequestTraceContext.GetTraceId(request),
        };
    }

    /// <summary>
    /// Creates a validation problem with messages keyed by JSON field path.
    /// </summary>
    public static ValidationProblemDetailsResponse CreateValidation(
        HttpRequestMessage request,
        IReadOnlyDictionary<string, string[]> errors)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (errors is null)
        {
            throw new ArgumentNullException(nameof(errors));
        }

        return new ValidationProblemDetailsResponse
        {
            Type = ProblemTypeUris.ValidationFailed,
            Title = "Request validation failed",
            Status = (int)HttpStatusCode.BadRequest,
            Detail = "One or more request fields are invalid.",
            Instance = GetInstance(request),
            TraceId = RequestTraceContext.GetTraceId(request),
            Errors = errors,
        };
    }

    /// <summary>
    /// Creates a safe response for an unexpected server failure without exposing exception details.
    /// </summary>
    public static ProblemDetailsResponse CreateInternalServerError(
        HttpRequestMessage request)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        return new ProblemDetailsResponse
        {
            Type = "about:blank",
            Title = "Internal Server Error",
            Status = (int)HttpStatusCode.InternalServerError,
            Detail = "An unexpected error occurred.",
            Instance = GetInstance(request),
            TraceId = RequestTraceContext.GetTraceId(request),
        };
    }

    private static string GetInstance(HttpRequestMessage request)
    {
        return request.RequestUri?.PathAndQuery ?? string.Empty;
    }
}
