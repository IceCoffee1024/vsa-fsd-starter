using System.Collections.Generic;

namespace BackendVsaOwin.BuildingBlocks.WebApi.Errors;

/// <summary>
/// Describes request validation failures and associates messages with JSON field paths.
/// </summary>
public sealed class ValidationProblemDetailsResponse : ProblemDetailsResponse
{
    /// <summary>
    /// Validation messages keyed by JSON field path.
    /// </summary>
    public required IReadOnlyDictionary<string, string[]> Errors { get; init; }
}
