using System.Security.Cryptography;
using System.Text;
using PasswordVault.Common.Interfaces;

namespace PasswordVault.Common.Services;

class CryptoFernet : ICrypto
{
    private string _secretKey;

    public CryptoFernet(string secretKey)
    {
        _secretKey = secretKey;
    }

    public string GetEncryptedText(string plainText)
    {
        return Cryptography.Fernet.Encrypt(_secretKey, plainText);
    }

    public string GetDecryptedText(string encryptedText)
    {
        return Cryptography.Fernet.Decrypt(_secretKey, encryptedText);
    }
}
