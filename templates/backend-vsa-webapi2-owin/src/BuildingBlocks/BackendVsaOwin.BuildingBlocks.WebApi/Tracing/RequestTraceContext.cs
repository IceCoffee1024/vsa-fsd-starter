using System;
using System.Diagnostics;
using System.Net.Http;

namespace BackendVsaOwin.BuildingBlocks.WebApi.Tracing;

/// <summary>
/// Provides the W3C trace identifier associated with the current HTTP request.
/// </summary>
public static class RequestTraceContext
{
    private const string TraceIdPropertyName =
        "BackendVsaOwin.BuildingBlocks.WebApi.TraceId";

    /// <summary>
    /// Returns the request trace identifier, creating one when the request did not pass through the trace handler.
    /// </summary>
    public static string GetTraceId(HttpRequestMessage request)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (request.Properties.TryGetValue(TraceIdPropertyName, out var value)
            && value is string storedTraceId)
        {
            return storedTraceId;
        }

        var currentTraceId = Activity.Current?.TraceId.ToString();
        var traceId = string.IsNullOrEmpty(currentTraceId)
            ? ActivityTraceId.CreateRandom().ToString()
            : currentTraceId!;

        SetTraceId(request, traceId);
        return traceId;
    }

    internal static void SetTraceId(
        HttpRequestMessage request,
        string traceId)
    {
        request.Properties[TraceIdPropertyName] = traceId;
    }
}
