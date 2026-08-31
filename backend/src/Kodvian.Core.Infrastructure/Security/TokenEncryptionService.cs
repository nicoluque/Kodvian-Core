using System.Security.Cryptography;
using System.Text;
using Kodvian.Core.Application.Common.Security;
using Microsoft.Extensions.Options;

namespace Kodvian.Core.Infrastructure.Security;

public class TokenEncryptionService : ITokenEncryptionService
{
    private const int NonceSize = 12;
    private const int TagSize = 16;
    private readonly Lazy<byte[]> _key;

    public TokenEncryptionService(IOptions<TokenEncryptionOptions> options)
    {
        _key = new Lazy<byte[]>(() => DeriveKey(options.Value.Key));
    }

    public string Encrypt(string plainText)
    {
        ArgumentException.ThrowIfNullOrEmpty(plainText);

        var nonce = RandomNumberGenerator.GetBytes(NonceSize);
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = new byte[plainBytes.Length];
        var tag = new byte[TagSize];

        using var aes = new AesGcm(_key.Value, TagSize);
        aes.Encrypt(nonce, plainBytes, cipherBytes, tag);

        var payload = new byte[NonceSize + cipherBytes.Length + TagSize];
        Buffer.BlockCopy(nonce, 0, payload, 0, NonceSize);
        Buffer.BlockCopy(cipherBytes, 0, payload, NonceSize, cipherBytes.Length);
        Buffer.BlockCopy(tag, 0, payload, NonceSize + cipherBytes.Length, TagSize);
        return Convert.ToBase64String(payload);
    }

    public string Decrypt(string cipherText)
    {
        ArgumentException.ThrowIfNullOrEmpty(cipherText);

        byte[] payload;
        try
        {
            payload = Convert.FromBase64String(cipherText);
        }
        catch (FormatException ex)
        {
            throw new CryptographicException("El texto cifrado no es válido.", ex);
        }

        if (payload.Length <= NonceSize + TagSize)
        {
            throw new CryptographicException("El texto cifrado es demasiado corto.");
        }

        var nonce = payload.AsSpan(0, NonceSize);
        var tag = payload.AsSpan(payload.Length - TagSize, TagSize);
        var cipherBytes = payload.AsSpan(NonceSize, payload.Length - NonceSize - TagSize);
        var plainBytes = new byte[cipherBytes.Length];

        try
        {
            using var aes = new AesGcm(_key.Value, TagSize);
            aes.Decrypt(nonce, cipherBytes, tag, plainBytes);
        }
        catch (CryptographicException ex)
        {
            throw new CryptographicException("No se pudo descifrar el token. Verificá TokenEncryption__Key.", ex);
        }

        return Encoding.UTF8.GetString(plainBytes);
    }

    public static byte[] DeriveKey(string? configuredKey)
    {
        if (string.IsNullOrWhiteSpace(configuredKey))
        {
            throw new InvalidOperationException("TokenEncryption__Key no está configurada.");
        }

        var trimmed = configuredKey.Trim();

        try
        {
            var decoded = Convert.FromBase64String(trimmed);
            if (decoded.Length == 32)
            {
                return decoded;
            }
        }
        catch (FormatException)
        {
            // Fall through to passphrase derivation.
        }

        if (trimmed.Length < 32)
        {
            throw new InvalidOperationException(
                "TokenEncryption__Key debe ser Base64 de 32 bytes o un secreto de al menos 32 caracteres.");
        }

        return SHA256.HashData(Encoding.UTF8.GetBytes(trimmed));
    }
}
