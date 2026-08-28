using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Owin.Security.DataProtection;

namespace BackendVsaOwin.Host.Authentication.DataProtection;

internal sealed class AesDataProtector : IDataProtector
{
    private const byte FormatVersion = 1;
    private const int IvLength = 16;
    private const int TagLength = 32;
    private readonly byte[] _encryptionKey;
    private readonly byte[] _authenticationKey;

    public AesDataProtector(byte[] masterKey, string[] purposes)
    {
        if (masterKey is null)
        {
            throw new ArgumentNullException(nameof(masterKey));
        }

        if (purposes is null)
        {
            throw new ArgumentNullException(nameof(purposes));
        }

        var purposeBytes = SerializePurposes(purposes);
        _encryptionKey = DeriveKey(masterKey, "encryption", purposeBytes);
        _authenticationKey = DeriveKey(masterKey, "authentication", purposeBytes);
    }

    public byte[] Protect(byte[] userData)
    {
        if (userData is null)
        {
            throw new ArgumentNullException(nameof(userData));
        }

        var iv = new byte[IvLength];
        using (var random = RandomNumberGenerator.Create())
        {
            random.GetBytes(iv);
        }

        byte[] ciphertext;
        using (var aes = CreateAes(iv))
        using (var encryptor = aes.CreateEncryptor())
        using (var output = new MemoryStream())
        using (var crypto = new CryptoStream(output, encryptor, CryptoStreamMode.Write))
        {
            crypto.Write(userData, 0, userData.Length);
            crypto.FlushFinalBlock();
            ciphertext = output.ToArray();
        }

        var protectedData = new byte[1 + iv.Length + ciphertext.Length + TagLength];
        protectedData[0] = FormatVersion;
        Buffer.BlockCopy(iv, 0, protectedData, 1, iv.Length);
        Buffer.BlockCopy(ciphertext, 0, protectedData, 1 + iv.Length, ciphertext.Length);
        var tag = ComputeTag(protectedData, 0, 1 + iv.Length + ciphertext.Length);
        Buffer.BlockCopy(tag, 0, protectedData, protectedData.Length - TagLength, TagLength);
        return protectedData;
    }

    public byte[] Unprotect(byte[] protectedData)
    {
        if (protectedData is null)
        {
            throw new ArgumentNullException(nameof(protectedData));
        }

        if (protectedData.Length < 1 + IvLength + TagLength
            || protectedData[0] != FormatVersion)
        {
            throw new CryptographicException("The protected data format is invalid.");
        }

        var authenticatedLength = protectedData.Length - TagLength;
        var expectedTag = ComputeTag(protectedData, 0, authenticatedLength);
        if (!FixedTimeEquals(expectedTag, protectedData, authenticatedLength, TagLength))
        {
            throw new CryptographicException("The protected data authentication tag is invalid.");
        }

        var iv = new byte[IvLength];
        Buffer.BlockCopy(protectedData, 1, iv, 0, iv.Length);
        var ciphertextLength = authenticatedLength - 1 - iv.Length;
        var ciphertext = new byte[ciphertextLength];
        Buffer.BlockCopy(protectedData, 1 + iv.Length, ciphertext, 0, ciphertext.Length);

        try
        {
            using (var aes = CreateAes(iv))
            using (var decryptor = aes.CreateDecryptor())
            using (var input = new MemoryStream(ciphertext))
            using (var crypto = new CryptoStream(input, decryptor, CryptoStreamMode.Read))
            using (var output = new MemoryStream())
            {
                crypto.CopyTo(output);
                return output.ToArray();
            }
        }
        catch (CryptographicException)
        {
            throw;
        }
        catch (Exception exception) when (exception is IOException || exception is ArgumentException)
        {
            throw new CryptographicException("The protected data could not be decrypted.", exception);
        }
    }

    private Aes CreateAes(byte[] iv)
    {
        var aes = Aes.Create();
        aes.Key = _encryptionKey;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        return aes;
    }

    private byte[] ComputeTag(byte[] data, int offset, int count)
    {
        using (var hmac = new HMACSHA256(_authenticationKey))
        {
            return hmac.ComputeHash(data, offset, count);
        }
    }

    private static byte[] DeriveKey(byte[] masterKey, string purpose, byte[] purposeBytes)
    {
        var domain = Encoding.UTF8.GetBytes("vsa-fsd-starter/aes-256/" + purpose + "\0");
        var input = new byte[domain.Length + purposeBytes.Length];
        Buffer.BlockCopy(domain, 0, input, 0, domain.Length);
        Buffer.BlockCopy(purposeBytes, 0, input, domain.Length, purposeBytes.Length);
        using (var hmac = new HMACSHA256(masterKey))
        {
            return hmac.ComputeHash(input);
        }
    }

    private static byte[] SerializePurposes(string[] purposes)
    {
        using (var output = new MemoryStream())
        using (var writer = new BinaryWriter(output, Encoding.UTF8))
        {
            foreach (var purpose in purposes)
            {
                if (purpose is null)
                {
                    throw new ArgumentException(
                        "Purpose values cannot be null.",
                        nameof(purposes));
                }

                var bytes = Encoding.UTF8.GetBytes(purpose);
                writer.Write(bytes.Length);
                writer.Write(bytes);
            }

            writer.Flush();
            return output.ToArray();
        }
    }

    private static bool FixedTimeEquals(
        byte[] expected,
        byte[] actual,
        int actualOffset,
        int length)
    {
        var difference = expected.Length ^ length;
        for (var index = 0; index < length; index++)
        {
            difference |= expected[index % expected.Length] ^ actual[actualOffset + index];
        }

        return difference == 0;
    }
}
