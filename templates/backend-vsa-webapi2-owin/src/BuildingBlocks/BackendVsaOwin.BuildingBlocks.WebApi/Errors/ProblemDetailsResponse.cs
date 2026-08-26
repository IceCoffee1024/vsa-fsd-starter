namespace BackendVsaOwin.BuildingBlocks.WebApi.Errors;

/// <summary>
/// Describes an HTTP API problem using the standard members defined by RFC 9457.
/// </summary>
public class ProblemDetailsResponse
{
    /// <summary>
    /// URI reference that identifies the problem type.
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// Short, human-readable summary of the problem type.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// HTTP status code generated for this occurrence of the problem.
    /// </summary>
    public required int Status { get; init; }

    /// <summary>
    /// Human-readable explanation specific to this occurrence of the problem.
    /// </summary>
    public required string Detail { get; init; }

    /// <summary>
    /// URI reference that identifies this occurrence of the problem.
    /// </summary>
    public required string Instance { get; init; }

    /// <summary>
    /// W3C trace identifier that correlates this response with server-side diagnostics.
    /// </summary>
    public required string TraceId { get; init; }
}
