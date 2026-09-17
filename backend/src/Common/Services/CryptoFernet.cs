using System.Security.Cryptography;
using System.Text;
using PasswordVault.Common.Interfaces;

namespace PasswordVault.Common.Services;

public class CryptoFernet : ICrypto
{
    private readonly string _secretKey;

    public CryptoFernet(string secretKey)
    {
        if (string.IsNullOrWhiteSpace(secretKey))
        {
            throw new ArgumentException("A Fernet key is required.", nameof(secretKey));
        }

        _secretKey = NormalizeKey(secretKey);
    }

    public string GetEncryptedText(string plainText)
    {
        return Cryptography.Fernet.Encrypt(_secretKey, plainText);
    }

    public string GetDecryptedText(string encryptedText)
    {
        return Cryptography.Fernet.Decrypt(_secretKey, encryptedText);
    }

    private static string NormalizeKey(string secretKey)
    {
        var bytes = Encoding.UTF8.GetBytes(secretKey);
        var keyBytes = SHA256.HashData(bytes);

        return Convert.ToBase64String(keyBytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
