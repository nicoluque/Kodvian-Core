namespace Kodvian.Core.Application.Profile.Dtos;

public class ProfileDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public Guid? DeveloperId { get; set; }
    public bool GitHubConnected { get; set; }
    public string? GitHubUsername { get; set; }
    public DateTime? GitHubConnectedAt { get; set; }
}
