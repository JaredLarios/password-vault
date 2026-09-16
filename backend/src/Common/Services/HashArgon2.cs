using PasswordVault.Common.Interfaces;
using Isopoh.Cryptography.Argon2;

namespace PasswordVault.Common.Services;

class HashArgon2 : IHash
{
    public string GetHash(string plainText)
    {
        return Argon2.Hash(plainText);
    }

    public bool CompareHash(string plainText, string hashedText)
    {
        return Argon2.Verify(hashedText, plainText);
    }
}
