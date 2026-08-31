using System.Security.Cryptography;
using System.Text;

namespace Kodvian.Core.Application.Integrations.GitHub;

public static class GitHubWebhookSignatureValidator
{
    public const string SignatureHeaderPrefix = "sha256=";

    public static bool IsValid(string payload, string? signatureHeader, string? secret)
    {
        if (string.IsNullOrWhiteSpace(secret)
            || string.IsNullOrWhiteSpace(signatureHeader)
            || string.IsNullOrWhiteSpace(payload))
        {
            return false;
        }

        if (!signatureHeader.StartsWith(SignatureHeaderPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var providedHex = signatureHeader[SignatureHeaderPrefix.Length..];
        if (providedHex.Length == 0)
        {
            return false;
        }

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        var expectedHex = Convert.ToHexString(hash).ToLowerInvariant();

        try
        {
            var providedBytes = Convert.FromHexString(providedHex);
            var expectedBytes = Convert.FromHexString(expectedHex);
            return providedBytes.Length == expectedBytes.Length
                && CryptographicOperations.FixedTimeEquals(providedBytes, expectedBytes);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    public static string ComputeSignature(string payload, string secret)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        return SignatureHeaderPrefix + Convert.ToHexString(hash).ToLowerInvariant();
    }
}
