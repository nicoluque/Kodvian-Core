using Kodvian.Core.Api.Validation;
using Kodvian.Core.Application.Common.Models;
using Kodvian.Core.Application.Developers.Abstractions;
using Kodvian.Core.Application.Developers.Dtos;
using Kodvian.Core.Application.Developers.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kodvian.Core.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/developers")]
public class DevelopersController : ControllerBase
{
    private readonly IDeveloperService _developerService;
    private readonly IProjectDeveloperContractService _contractService;

    public DevelopersController(IDeveloperService developerService, IProjectDeveloperContractService contractService)
    {
        _developerService = developerService;
        _contractService = contractService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<IReadOnlyCollection<DeveloperDto>>>> GetAll(CancellationToken cancellationToken)
    {
        var data = await _developerService.GetAllAsync(cancellationToken);
        return Ok(ApiResponseDto<IReadOnlyCollection<DeveloperDto>>.Ok(data, "Desarrolladores obtenidos correctamente"));
    }

    [HttpGet("{id:guid}/contracts-summary")]
    public async Task<ActionResult<ApiResponseDto<IReadOnlyCollection<DeveloperContractSummaryDto>>>> GetContractsSummary(Guid id, [FromQuery] int? year, CancellationToken cancellationToken)
    {
        var periodYear = year ?? DateTime.UtcNow.Year;
        if (periodYear is < 2000 or > 2100)
        {
            return BadRequest(ApiResponseDto<IReadOnlyCollection<DeveloperContractSummaryDto>>.Fail("El año debe estar entre 2000 y 2100"));
        }

        var data = await _contractService.GetByDeveloperSummaryAsync(id, periodYear, cancellationToken);
        return Ok(ApiResponseDto<IReadOnlyCollection<DeveloperContractSummaryDto>>.Ok(data, "Resumen de contratos obtenido correctamente"));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<DeveloperDto>>> Create([FromBody] DeveloperUpsertRequestDto request, CancellationToken cancellationToken)
    {
        var validationError = RequestValidation.Validate(request);
        if (validationError is not null)
        {
            return BadRequest(ApiResponseDto<DeveloperDto>.Fail(validationError));
        }

        var data = await _developerService.CreateAsync(request, cancellationToken);
        return Ok(ApiResponseDto<DeveloperDto>.Ok(data, "El desarrollador se creó correctamente"));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponseDto<DeveloperDto>>> Update(Guid id, [FromBody] DeveloperUpsertRequestDto request, CancellationToken cancellationToken)
    {
        var validationError = RequestValidation.Validate(request);
        if (validationError is not null)
        {
            return BadRequest(ApiResponseDto<DeveloperDto>.Fail(validationError));
        }

        var data = await _developerService.UpdateAsync(id, request, cancellationToken);
        if (data is null)
        {
            return NotFound(ApiResponseDto<DeveloperDto>.Fail("Desarrollador no encontrado"));
        }

        return Ok(ApiResponseDto<DeveloperDto>.Ok(data, "El desarrollador se actualizó correctamente"));
    }
}
