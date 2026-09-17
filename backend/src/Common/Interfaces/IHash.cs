namespace PasswordVault.Common.Interfaces;

public interface IHash
{
    string GetHash(string plainText);
    bool CompareHash(string plainText, string hashedText);
}
