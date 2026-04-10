using Kodvian.Core.Application.Common.Models;
using Kodvian.Core.Application.Dashboard.Abstractions;
using Kodvian.Core.Application.Dashboard.Dtos;
using Kodvian.Core.Application.Dashboard.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kodvian.Core.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("overview")]
    public async Task<ActionResult<ApiResponseDto<DashboardOverviewDto>>> GetOverview([FromQuery] DashboardOverviewRequestDto request, CancellationToken cancellationToken)
    {
        if (request.Year.HasValue && (request.Year.Value < 2000 || request.Year.Value > 2100))
        {
            return BadRequest(ApiResponseDto<DashboardOverviewDto>.Fail("El año debe estar entre 2000 y 2100"));
        }

        if (request.Month.HasValue && (request.Month.Value < 1 || request.Month.Value > 12))
        {
            return BadRequest(ApiResponseDto<DashboardOverviewDto>.Fail("El mes debe estar entre 1 y 12"));
        }

        var data = await _dashboardService.GetOverviewAsync(request, cancellationToken);
        return Ok(ApiResponseDto<DashboardOverviewDto>.Ok(data, "Resumen del inicio obtenido correctamente"));
    }
}
