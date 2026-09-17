namespace PasswordVault.Common.Interfaces;

public interface ICrypto
{
    string GetEncryptedText(string plainText);
    string GetDecryptedText(string encryptedText);
}
