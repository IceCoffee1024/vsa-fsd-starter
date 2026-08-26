using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.ModelBinding;

namespace BackendVsaOwin.BuildingBlocks.WebApi.Validation;

/// <summary>
/// Converts Web API binding errors into the transport-neutral validation shape
/// used by the template's Problem Details responses.
/// </summary>
public static class ModelStateErrorMapper
{
    /// <summary>
    /// Maps binding and conversion errors without exposing formatter exception details.
    /// </summary>
    public static IReadOnlyDictionary<string, string[]> ToErrors(
        ModelStateDictionary modelState)
    {
        if (modelState is null)
        {
            throw new ArgumentNullException(nameof(modelState));
        }

        var errors = new Dictionary<string, string[]>();

        foreach (var entry in modelState)
        {
            var messages = entry.Value.Errors
                .Select(error => GetSafeMessage(entry.Key, error.ErrorMessage))
                .ToArray();
            if (messages.Length == 0)
            {
                continue;
            }

            var key = ToJsonFieldPath(entry.Key);
            if (errors.TryGetValue(key, out var existing))
            {
                errors[key] = existing.Concat(messages).ToArray();
            }
            else
            {
                errors[key] = messages;
            }
        }

        return errors;
    }

    private static string GetSafeMessage(string key, string? errorMessage)
    {
        if (!string.IsNullOrWhiteSpace(errorMessage))
        {
            return errorMessage!;
        }

        return string.Equals(key, "request", StringComparison.OrdinalIgnoreCase)
            ? "The request body is invalid."
            : "The supplied value is invalid.";
    }

    private static string ToJsonFieldPath(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return "request";
        }

        var characters = key.ToCharArray();
        var segmentStart = true;

        for (var index = 0; index < characters.Length; index++)
        {
            if (segmentStart && char.IsLetter(characters[index]))
            {
                characters[index] = char.ToLowerInvariant(characters[index]);
                segmentStart = false;
            }
            else if (characters[index] == '.')
            {
                segmentStart = true;
            }
        }

        return new string(characters);
    }
}
