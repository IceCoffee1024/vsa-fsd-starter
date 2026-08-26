using System;
using System.Configuration;
using System.Text;

namespace BackendVsaOwin.Host.Authentication;

internal sealed class ConfiguredCredentialValidator : ICredentialValidator
{
    private readonly byte[] _username;
    private readonly byte[] _password;

    public ConfiguredCredentialValidator(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentException(
                "A non-empty value is required.",
                nameof(username));
        }

        if (username.Contains(":"))
        {
            throw new ArgumentException(
                "A Basic-authentication username cannot contain a colon.",
                nameof(username));
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException(
                "A non-empty value is required.",
                nameof(password));
        }

        _username = Encoding.UTF8.GetBytes(username);
        _password = Encoding.UTF8.GetBytes(password);
    }

    public static ConfiguredCredentialValidator FromConfiguration()
    {
        return new ConfiguredCredentialValidator(
            ConfigurationManager.AppSettings["Username"]
                ?? throw new ConfigurationErrorsException(
                    "AppSetting 'Username' is required."),
            ConfigurationManager.AppSettings["Password"]
                ?? throw new ConfigurationErrorsException(
                    "AppSetting 'Password' is required."));
    }

    public bool Validate(string username, string password)
    {
        var usernameMatches = FixedTimeEquals(
            Encoding.UTF8.GetBytes(username),
            _username);
        var passwordMatches = FixedTimeEquals(
            Encoding.UTF8.GetBytes(password),
            _password);

        // Evaluate both comparisons so an unknown username does not skip password work.
        return usernameMatches & passwordMatches;
    }

    private static bool FixedTimeEquals(byte[] left, byte[] right)
    {
        var difference = left.Length ^ right.Length;
        var length = Math.Max(left.Length, right.Length);

        for (var index = 0; index < length; index++)
        {
            var leftByte = index < left.Length ? left[index] : (byte)0;
            var rightByte = index < right.Length ? right[index] : (byte)0;
            difference |= leftByte ^ rightByte;
        }

        return difference == 0;
    }
}
