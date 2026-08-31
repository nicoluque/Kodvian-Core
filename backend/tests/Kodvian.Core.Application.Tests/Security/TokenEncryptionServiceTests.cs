using System.Security.Cryptography;
using Kodvian.Core.Infrastructure.Security;
using Microsoft.Extensions.Options;

namespace Kodvian.Core.Application.Tests.Security;

public class TokenEncryptionServiceTests
{
    [Fact]
    public void EncryptDecrypt_RoundTrip_ReturnsOriginalValue()
    {
        var service = CreateService("local-dev-token-encryption-key-32chars!");

        var cipher = service.Encrypt("gho_example_oauth_token");
        var plain = service.Decrypt(cipher);

        Assert.NotEqual("gho_example_oauth_token", cipher);
        Assert.Equal("gho_example_oauth_token", plain);
    }

    [Fact]
    public void Encrypt_ProducesDifferentCiphertexts_ForSameInput()
    {
        var service = CreateService("local-dev-token-encryption-key-32chars!");

        var first = service.Encrypt("same-token");
        var second = service.Encrypt("same-token");

        Assert.NotEqual(first, second);
    }

    [Fact]
    public void DeriveKey_Throws_WhenKeyMissing()
    {
        var exception = Assert.Throws<InvalidOperationException>(() => TokenEncryptionService.DeriveKey(" "));

        Assert.Contains("TokenEncryption__Key", exception.Message);
    }

    [Fact]
    public void DeriveKey_Throws_WhenKeyTooShort()
    {
        var exception = Assert.Throws<InvalidOperationException>(() => TokenEncryptionService.DeriveKey("short-key"));

        Assert.Contains("32", exception.Message);
    }

    [Fact]
    public void Decrypt_Throws_WhenCiphertextTampered()
    {
        var service = CreateService("local-dev-token-encryption-key-32chars!");
        var cipher = service.Encrypt("secret-token");
        var bytes = Convert.FromBase64String(cipher);
        bytes[^1] ^= 0xFF;
        var tampered = Convert.ToBase64String(bytes);

        Assert.Throws<CryptographicException>(() => service.Decrypt(tampered));
    }

    [Fact]
    public void DeriveKey_AcceptsBase64ThirtyTwoBytes()
    {
        var keyBytes = RandomNumberGenerator.GetBytes(32);
        var key = Convert.ToBase64String(keyBytes);

        var derived = TokenEncryptionService.DeriveKey(key);

        Assert.Equal(keyBytes, derived);
    }

    private static TokenEncryptionService CreateService(string key)
    {
        return new TokenEncryptionService(Options.Create(new TokenEncryptionOptions { Key = key }));
    }
}
