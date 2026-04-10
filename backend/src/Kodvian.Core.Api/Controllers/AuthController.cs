using System.Security.Claims;
using Kodvian.Core.Application.Auth.Abstractions;
using Kodvian.Core.Application.Auth.Dtos;
using Kodvian.Core.Application.Common.Models;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kodvian.Core.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private const string AuthCookieName = "auth_token";

    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("AuthLogin")]
    public async Task<ActionResult<ApiResponseDto<LoginResponseDto>>> Login([FromBody] LoginRequestDto request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(ApiResponseDto<LoginResponseDto>.Fail("Email y contraseña son obligatorios"));
        }

        var loginResponse = await _authService.LoginAsync(request, cancellationToken);
        if (loginResponse is null)
        {
            return Unauthorized(ApiResponseDto<LoginResponseDto>.Fail("Credenciales inválidas"));
        }

        Response.Cookies.Append(AuthCookieName, loginResponse.AccessToken, BuildCookieOptions(loginResponse.ExpiresAtUtc));
        loginResponse.AccessToken = string.Empty;

        return Ok(ApiResponseDto<LoginResponseDto>.Ok(loginResponse, "Inicio de sesión correcto"));
    }

    [HttpPost("logout")]
    [Authorize]
    public ActionResult<ApiResponseDto<object>> Logout()
    {
        Response.Cookies.Delete(AuthCookieName, BuildCookieOptions(DateTime.UtcNow.AddMinutes(-5)));
        return Ok(ApiResponseDto<object>.Ok(new { }, "Sesión cerrada correctamente"));
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<ApiResponseDto<CurrentUserDto>>> Me(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponseDto<CurrentUserDto>.Fail("Token inválido"));
        }

        var user = await _authService.GetCurrentUserAsync(userId, cancellationToken);
        if (user is null)
        {
            return NotFound(ApiResponseDto<CurrentUserDto>.Fail("Usuario no encontrado"));
        }

        return Ok(ApiResponseDto<CurrentUserDto>.Ok(user, "Usuario autenticado obtenido correctamente"));
    }

    private CookieOptions BuildCookieOptions(DateTime expiresAtUtc)
    {
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Strict,
            IsEssential = true,
            Path = "/",
            Expires = new DateTimeOffset(expiresAtUtc)
        };
    }
}
