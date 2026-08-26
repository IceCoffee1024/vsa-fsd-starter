using System;
using Microsoft.Owin.Security;

namespace BackendVsaOwin.Host.Authentication;

/// <summary>
/// Configures the Katana Basic authentication scheme and challenge realm.
/// </summary>
internal sealed class BasicAuthenticationOptions : AuthenticationOptions
{
    public const string Scheme = "Basic";

    public BasicAuthenticationOptions(string realm)
        : base(Scheme)
    {
        Realm = RequireValue(realm, nameof(realm));
        AuthenticationMode = AuthenticationMode.Active;
    }

    public string Realm { get; }

    private static string RequireValue(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "A non-empty value is required.",
                parameterName);
        }

        return value;
    }
}
