namespace Kodvian.Core.Application.Auth.Dtos;

public class LoginResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public CurrentUserDto User { get; set; } = new();
}
