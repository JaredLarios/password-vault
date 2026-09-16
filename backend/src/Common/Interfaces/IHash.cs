namespace PasswordVault.Common.Interfaces;

interface IHash
{
    string GetHash(string plainText);
    bool CompareHash(string plainText, string hashedText);
}
