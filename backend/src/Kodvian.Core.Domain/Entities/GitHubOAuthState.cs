namespace Kodvian.Core.Domain.Entities;

public class GitHubOAuthState
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string StateToken { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
}
