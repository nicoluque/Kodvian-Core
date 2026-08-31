using System.Security.Claims;
using Kodvian.Core.Application.Common.Models;
using Kodvian.Core.Application.Profile.Abstractions;
using Kodvian.Core.Application.Profile.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kodvian.Core.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/profile")]
public class ProfileController : ControllerBase
{
    private readonly IProfileService _profileService;

    public ProfileController(IProfileService profileService)
    {
        _profileService = profileService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<ProfileDto>>> GetProfile(CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized(ApiResponseDto<ProfileDto>.Fail("Token inválido"));
        }

        var profile = await _profileService.GetProfileAsync(userId, cancellationToken);
        if (profile is null)
        {
            return NotFound(ApiResponseDto<ProfileDto>.Fail("Usuario no encontrado"));
        }

        return Ok(ApiResponseDto<ProfileDto>.Ok(profile, "Perfil obtenido correctamente"));
    }

    [HttpGet("github/connect")]
    public async Task<IActionResult> ConnectGitHub(CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized(ApiResponseDto<object>.Fail("Token inválido"));
        }

        try
        {
            var authorizeUrl = await _profileService.CreateGitHubConnectUrlAsync(userId, cancellationToken);
            return Redirect(authorizeUrl);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponseDto<object>.Fail(ex.Message));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponseDto<object>.Fail(ex.Message));
        }
    }

    [HttpGet("github/callback")]
    [AllowAnonymous]
    public async Task<IActionResult> GitHubCallback([FromQuery] string? code, [FromQuery] string? state, CancellationToken cancellationToken)
    {
        try
        {
            var relativePath = await _profileService.CompleteGitHubCallbackAsync(code, state, cancellationToken);
            return Redirect(BuildFrontendAbsoluteUrl(relativePath));
        }
        catch (ArgumentException)
        {
            return Redirect(BuildFrontendAbsoluteUrl("/mi-perfil?connected=false&error=oauth"));
        }
        catch (InvalidOperationException)
        {
            return Redirect(BuildFrontendAbsoluteUrl("/mi-perfil?connected=false&error=disabled"));
        }
        catch (Exception)
        {
            return Redirect(BuildFrontendAbsoluteUrl("/mi-perfil?connected=false&error=github"));
        }
    }

    [HttpDelete("github/disconnect")]
    public async Task<ActionResult<ApiResponseDto<object>>> DisconnectGitHub(CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized(ApiResponseDto<object>.Fail("Token inválido"));
        }

        try
        {
            await _profileService.DisconnectGitHubAsync(userId, cancellationToken);
            return Ok(ApiResponseDto<object>.Ok(new { }, "Cuenta de GitHub desconectada correctamente"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponseDto<object>.Fail(ex.Message));
        }
    }

    private bool TryGetUserId(out Guid userId)
    {
        var claimValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(claimValue, out userId);
    }

    private string BuildFrontendAbsoluteUrl(string relativePath)
    {
        var path = string.IsNullOrWhiteSpace(relativePath) ? "/mi-perfil" : relativePath;
        if (!path.StartsWith('/'))
        {
            path = "/" + path;
        }

        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        return baseUrl.TrimEnd('/') + path;
    }
}
