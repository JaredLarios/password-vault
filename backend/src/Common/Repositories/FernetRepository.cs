using System.Text;
using System.Security.Cryptography;
using PasswordVault.Common.Interfaces;

namespace PasswordVault.Common.Repositories;

public class FernetRepository : ICrypto
{
    private readonly string _secretKey;

    public FernetRepository(string secretKey)
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
