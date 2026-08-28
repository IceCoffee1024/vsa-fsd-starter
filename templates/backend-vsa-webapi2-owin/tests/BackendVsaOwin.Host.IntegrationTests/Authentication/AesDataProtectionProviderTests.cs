using System;
using System.Configuration;
using System.IO;
using System.Security.Cryptography;
using System.Linq;
using System.Xml.Linq;
using BackendVsaOwin.Host.Authentication.DataProtection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace BackendVsaOwin.Host.IntegrationTests.Authentication;

public sealed class AesDataProtectionProviderTests
{
    private static readonly byte[] Key =
        new byte[32]
        {
            0, 1, 2, 3, 4, 5, 6, 7,
            8, 9, 10, 11, 12, 13, 14, 15,
            16, 17, 18, 19, 20, 21, 22, 23,
            24, 25, 26, 27, 28, 29, 30, 31,
        };

    [Fact]
    public void Protect_round_trips_and_uses_a_random_iv()
    {
        var protector = new AesDataProtectionProvider(Key)
            .Create("tests", "round-trip");
        var plaintext = new byte[] { 1, 2, 3, 4 };

        var first = protector.Protect(plaintext);
        var second = protector.Protect(plaintext);

        Assert.Equal(plaintext, protector.Unprotect(first));
        Assert.Equal(plaintext, protector.Unprotect(second));
        Assert.NotEqual(first, second);
    }

    [Fact]
    public void Different_purposes_and_keys_cannot_unprotect_data()
    {
        var protector = new AesDataProtectionProvider(Key)
            .Create("tests", "purpose-a");
        var differentPurpose = new AesDataProtectionProvider(Key)
            .Create("tests", "purpose-b");
        var differentKey = new AesDataProtectionProvider(CreateOtherKey())
            .Create("tests", "purpose-a");
        var protectedData = protector.Protect(new byte[] { 9, 8, 7 });

        Assert.Throws<CryptographicException>(
            () => differentPurpose.Unprotect(protectedData));
        Assert.Throws<CryptographicException>(
            () => differentKey.Unprotect(protectedData));
    }

    [Fact]
    public void Tampering_is_rejected()
    {
        var protector = new AesDataProtectionProvider(Key).Create("tests");
        var protectedData = protector.Protect(new byte[] { 9, 8, 7 });
        protectedData[protectedData.Length - 1] ^= 1;

        Assert.Throws<CryptographicException>(
            () => protector.Unprotect(protectedData));
    }

    [Fact]
    public void Key_must_be_exactly_32_bytes()
    {
        Assert.Throws<ArgumentException>(
            () => new AesDataProtectionProvider(new byte[31]));
    }

    [Fact]
    public void Missing_key_is_generated_and_persisted_to_configuration()
    {
        var path = Path.GetTempFileName();
        File.WriteAllText(
            path,
            "<configuration><appSettings><add key=\"DataProtectionKey\" value=\"\" /></appSettings></configuration>");

        try
        {
            var provider = AesDataProtectionProvider.FromConfiguration(
                path,
                NullLogger.Instance);
            var document = XDocument.Load(path);
            var encodedKey = document.Root?
                .Element("appSettings")?
                .Elements("add")
                .First(element => (string)element.Attribute("key") == "DataProtectionKey")
                .Attribute("value")?
                .Value;

            Assert.False(string.IsNullOrWhiteSpace(encodedKey));
            Assert.Equal(32, Convert.FromBase64String(encodedKey).Length);
            Assert.Equal(
                new byte[] { 1, 2, 3 },
                provider.Create("configuration").Unprotect(
                    provider.Create("configuration").Protect(new byte[] { 1, 2, 3 })));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Existing_key_is_loaded_from_configuration()
    {
        var path = Path.GetTempFileName();
        var encodedKey = Convert.ToBase64String(Key);
        File.WriteAllText(
            path,
            $"<configuration><appSettings><add key=\"DataProtectionKey\" value=\"{encodedKey}\" /></appSettings></configuration>");

        try
        {
            var provider = AesDataProtectionProvider.FromConfiguration(
                path,
                NullLogger.Instance);
            var protector = provider.Create("configuration");
            var protectedData = protector.Protect(new byte[] { 4, 5, 6 });

            Assert.Equal(new byte[] { 4, 5, 6 }, protector.Unprotect(protectedData));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Invalid_configured_key_length_is_rejected()
    {
        var path = Path.GetTempFileName();
        var encodedKey = Convert.ToBase64String(new byte[31]);
        File.WriteAllText(
            path,
            $"<configuration><appSettings><add key=\"DataProtectionKey\" value=\"{encodedKey}\" /></appSettings></configuration>");

        try
        {
            Assert.Throws<ConfigurationErrorsException>(
                () => AesDataProtectionProvider.FromConfiguration(
                    path,
                    NullLogger.Instance));
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static byte[] CreateOtherKey()
    {
        var key = (byte[])Key.Clone();
        key[0] = 255;
        return key;
    }
}
