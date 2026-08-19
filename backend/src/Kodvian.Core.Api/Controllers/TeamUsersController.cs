using Kodvian.Core.Application.Common.Models;
using Kodvian.Core.Application.Team.Abstractions;
using Kodvian.Core.Application.Team.Dtos;
using Kodvian.Core.Application.Team.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kodvian.Core.Api.Controllers;

[ApiController]
[Route("api/team/users")]
public class TeamUsersController : ControllerBase
{
    private readonly ITeamUserService _teamUserService;

    public TeamUsersController(ITeamUserService teamUserService)
    {
        _teamUserService = teamUserService;
    }

    [HttpGet("analysts")]
    [Authorize(Policy = "TeamRead")]
    public async Task<ActionResult<ApiResponseDto<IReadOnlyCollection<TeamUserDto>>>> GetAnalysts(CancellationToken cancellationToken)
    {
        var analysts = await _teamUserService.GetAnalystsAsync(cancellationToken);
        return Ok(ApiResponseDto<IReadOnlyCollection<TeamUserDto>>.Ok(analysts, "Analistas obtenidos correctamente"));
    }

    [HttpPost("analysts")]
    [Authorize(Policy = "TeamWrite")]
    public async Task<ActionResult<ApiResponseDto<TeamUserDto>>> CreateAnalyst(
        [FromBody] TeamUserUpsertRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var analyst = await _teamUserService.CreateAnalystAsync(request, cancellationToken);
            return Created($"/api/team/users/analysts/{analyst.Id}", ApiResponseDto<TeamUserDto>.Ok(analyst, "Analista creado correctamente"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponseDto<TeamUserDto>.Fail(ex.Message));
        }
    }

    [HttpPut("analysts/{id:guid}")]
    [Authorize(Policy = "TeamWrite")]
    public async Task<ActionResult<ApiResponseDto<TeamUserDto>>> UpdateAnalyst(
        Guid id,
        [FromBody] TeamUserUpsertRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var analyst = await _teamUserService.UpdateAnalystAsync(id, request, cancellationToken);
            if (analyst is null)
            {
                return NotFound(ApiResponseDto<TeamUserDto>.Fail("Analista no encontrado"));
            }

            return Ok(ApiResponseDto<TeamUserDto>.Ok(analyst, "Analista actualizado correctamente"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponseDto<TeamUserDto>.Fail(ex.Message));
        }
    }
}
