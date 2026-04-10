namespace Kodvian.Core.Application.Auth.Dtos;

public class TokenGenerationDto
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public IReadOnlyCollection<string> Permissions { get; set; } = Array.Empty<string>();
}
