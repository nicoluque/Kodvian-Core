namespace Kodvian.Core.Infrastructure.Integrations.GitHub;

public class GitHubOptions
{
    public const string SectionName = "GitHub";

    public bool Enabled { get; set; }
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string CallbackUrl { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
    public string ApiBaseUrl { get; set; } = "https://api.github.com";
    public string DefaultLabel { get; set; } = "kodvian";
    public string? ServiceToken { get; set; }
    public string OAuthScope { get; set; } = "read:user repo";
    public string FrontendSuccessPath { get; set; } = "/mi-perfil?connected=true";
    public string FrontendErrorPath { get; set; } = "/mi-perfil?connected=false";
    public int OAuthStateExpirationMinutes { get; set; } = 10;
}
