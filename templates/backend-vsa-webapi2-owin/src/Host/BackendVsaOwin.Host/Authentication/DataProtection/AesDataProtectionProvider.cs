using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Xml.Linq;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Extensions.Logging;

namespace BackendVsaOwin.Host.Authentication.DataProtection;

internal sealed class AesDataProtectionProvider : IDataProtectionProvider
{
    private const string KeySettingName = "DataProtectionKey";
    private readonly byte[] _masterKey;

    public AesDataProtectionProvider(byte[] masterKey)
    {
        if (masterKey is null)
        {
            throw new ArgumentNullException(nameof(masterKey));
        }

        if (masterKey.Length != 32)
        {
            throw new ArgumentException(
                "The data protection key must contain exactly 32 bytes.",
                nameof(masterKey));
        }

        _masterKey = (byte[])masterKey.Clone();
    }

    public static AesDataProtectionProvider FromConfiguration(ILogger logger)
    {
        if (logger is null)
        {
            throw new ArgumentNullException(nameof(logger));
        }

        var configPath = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
        return FromConfiguration(configPath, logger);
    }

    internal static AesDataProtectionProvider FromConfiguration(
        string configPath,
        ILogger logger)
    {
        if (string.IsNullOrWhiteSpace(configPath))
        {
            throw new ArgumentException(
                "A configuration file path is required.",
                nameof(configPath));
        }

        if (logger is null)
        {
            throw new ArgumentNullException(nameof(logger));
        }

        XDocument document;
        try
        {
            document = XDocument.Load(configPath, LoadOptions.PreserveWhitespace);
        }
        catch (Exception exception) when (exception is IOException
                                           || exception is UnauthorizedAccessException
                                           || exception is System.Xml.XmlException)
        {
            throw new ConfigurationErrorsException(
                $"The configuration file '{configPath}' could not be loaded.",
                exception);
        }

        var root = document.Root
            ?? throw new ConfigurationErrorsException(
                "The configuration file has no root element.");
        var appSettings = root.Element("appSettings");
        if (appSettings is null)
        {
            appSettings = new XElement("appSettings");
            root.AddFirst(appSettings);
        }

        var keyElement = appSettings
            .Elements("add")
            .FirstOrDefault(element => string.Equals(
                (string)element.Attribute("key"),
                KeySettingName,
                StringComparison.Ordinal));
        if (keyElement is null)
        {
            keyElement = new XElement(
                "add",
                new XAttribute("key", KeySettingName),
                new XAttribute("value", string.Empty));
            appSettings.Add(keyElement);
        }

        var encodedKey = (string)keyElement.Attribute("value");
        if (string.IsNullOrWhiteSpace(encodedKey))
        {
            var generatedKey = GenerateKey();
            encodedKey = Convert.ToBase64String(generatedKey);
            keyElement.SetAttributeValue("value", encodedKey);
            document.Save(configPath, SaveOptions.DisableFormatting);
            ConfigurationManager.RefreshSection("appSettings");
            logger.LogWarning(
                "Generated and persisted a new data-protection key. Key fingerprint: {Fingerprint}",
                Fingerprint(generatedKey));
            return new AesDataProtectionProvider(generatedKey);
        }

        byte[] key;
        try
        {
            key = Convert.FromBase64String(encodedKey);
        }
        catch (FormatException exception)
        {
            throw new ConfigurationErrorsException(
                $"The '{KeySettingName}' app setting must be valid Base64.",
                exception);
        }

        var fingerprint = Fingerprint(key);
        if (key.Length != 32)
        {
            logger.LogWarning(
                "The configured data-protection key has invalid length {Length} bytes. Key fingerprint: {Fingerprint}",
                key.Length,
                fingerprint);
            throw new ConfigurationErrorsException(
                $"The '{KeySettingName}' app setting must decode to exactly 32 bytes.");
        }

        logger.LogInformation(
            "Loaded the configured data-protection key. Key fingerprint: {Fingerprint}",
            fingerprint);
        return new AesDataProtectionProvider(key);
    }

    public IDataProtector Create(params string[] purposes)
    {
        return new AesDataProtector(_masterKey, purposes);
    }

    private static byte[] GenerateKey()
    {
        var key = new byte[32];
        using (var random = RandomNumberGenerator.Create())
        {
            random.GetBytes(key);
        }

        return key;
    }

    private static string Fingerprint(byte[] key)
    {
        using (var sha256 = SHA256.Create())
        {
            return BitConverter.ToString(sha256.ComputeHash(key))
                .Replace("-", string.Empty)
                .ToLowerInvariant();
        }
    }
}
