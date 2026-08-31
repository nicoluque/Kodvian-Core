namespace Kodvian.Core.Infrastructure.Security;

public class TokenEncryptionOptions
{
    public const string SectionName = "TokenEncryption";

    public string Key { get; set; } = string.Empty;
}
