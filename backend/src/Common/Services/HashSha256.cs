using System.Security.Cryptography;
using System.Text;
using PasswordVault.Common.Interfaces;

namespace PasswordVault.Common.Services;

public class HashSha256 : IHash
{
    public bool CompareHash(string plainText, string hashedText)
    {
        byte[] hashedTextBytes = Encoding.UTF8.GetBytes(hashedText);
        byte[] hashBytes = Encoding.UTF8.GetBytes(
            GetHash(plainText)
        );
        return CryptographicOperations.FixedTimeEquals(hashBytes, hashedTextBytes);
    }

    public string GetHash(string plainText)
    {
        byte[] hashBytes = SHA256.HashData(
            Encoding.UTF8.GetBytes(plainText)
        );

        return Convert.ToHexStringLower(hashBytes);
    }
}
