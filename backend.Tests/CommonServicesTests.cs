using PasswordVault.Common.Repositories;

namespace PasswordVaultAPI.Tests;

public class CommonServicesTests
{
    [Fact]
    public void Sha1Hash_IsDeterministicAndCanBeCompared()
    {
        var service = new Sha1Repository();

        string hash = service.GetHash("password");

        Assert.Equal("5baa61e4c9b93f3f0682250b6cf8331b7ee68fd8", hash);
        Assert.True(service.CompareHash("password", hash));
        Assert.False(service.CompareHash("wrong", hash));
        Assert.False(service.CompareHash("password", "short"));
    }

    [Fact]
    public void Sha256Hash_IsDeterministicAndCanBeCompared()
    {
        var service = new Sha256Repository();

        string hash = service.GetHash("password");

        Assert.Equal("5e884898da28047151d0e56f8dc6292773603d0d6aabbdd62a11ef721d1542d8", hash);
        Assert.True(service.CompareHash("password", hash));
        Assert.False(service.CompareHash("wrong", hash));
        Assert.False(service.CompareHash("password", "short"));
    }

    [Fact]
    public void Argon2Hash_CanBeGeneratedAndVerified()
    {
        var service = new Argon2Repository();

        string hash = service.GetHash("password");

        Assert.NotEmpty(hash);
        Assert.True(service.CompareHash("password", hash));
        Assert.False(service.CompareHash("wrong", hash));
    }

    [Fact]
    public void Fernet_CanEncryptAndDecryptText()
    {
        var service = new FernetRepository("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=");

        string encrypted = service.GetEncryptedText("secret text");

        Assert.NotEqual("secret text", encrypted);
        Assert.Equal("secret text", service.GetDecryptedText(encrypted));
    }
}
