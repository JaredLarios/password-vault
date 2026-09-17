namespace PasswordVault.Common.Interfaces;

interface ICrypto
{
    string GetEncryptedText(string plainText);
    string GetDecryptedText(string encryptedText);
}
