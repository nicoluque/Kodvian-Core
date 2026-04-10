namespace Kodvian.Core.Application.Auth.Dtos;

public class CurrentUserDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public IReadOnlyCollection<string> Permissions { get; set; } = Array.Empty<string>();
}
