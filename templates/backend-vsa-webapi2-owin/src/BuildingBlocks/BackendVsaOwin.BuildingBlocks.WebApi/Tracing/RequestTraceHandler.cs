using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace BackendVsaOwin.BuildingBlocks.WebApi.Tracing;

/// <summary>
/// Establishes a W3C trace context for each request and exposes its trace identifier in the response.
/// </summary>
public sealed class RequestTraceHandler : DelegatingHandler
{
    /// <summary>
    /// Response header that carries the request trace identifier.
    /// </summary>
    public const string TraceIdHeaderName = "X-Trace-Id";

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        using var activity = CreateActivity(request);
        activity.Start();

        var traceId = activity.TraceId.ToString();
        RequestTraceContext.SetTraceId(request, traceId);

        try
        {
            var response = await base
                .SendAsync(request, cancellationToken)
                .ConfigureAwait(false);

            response.Headers.Remove(TraceIdHeaderName);
            response.Headers.TryAddWithoutValidation(
                TraceIdHeaderName,
                traceId);

            return response;
        }
        finally
        {
            activity.Stop();
        }
    }

    private static Activity CreateActivity(HttpRequestMessage request)
    {
        var activity = new Activity(
                $"{request.Method.Method} {request.RequestUri?.AbsolutePath}")
            .SetIdFormat(ActivityIdFormat.W3C);

        var traceParent = GetSingleHeader(request, "traceparent");
        var traceState = GetSingleHeader(request, "tracestate");
        if (ActivityContext.TryParse(
                traceParent,
                traceState,
                isRemote: true,
                out var parentContext))
        {
            activity.SetParentId(traceParent!);
            activity.TraceStateString = parentContext.TraceState;
        }

        return activity;
    }

    private static string? GetSingleHeader(
        HttpRequestMessage request,
        string headerName)
    {
        return request.Headers.TryGetValues(headerName, out var values)
            ? values.FirstOrDefault()
            : null;
    }
}
