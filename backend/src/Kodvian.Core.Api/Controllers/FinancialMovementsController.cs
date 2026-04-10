using System.Security.Claims;
using Kodvian.Core.Api.Validation;
using Kodvian.Core.Application.Common.Models;
using Kodvian.Core.Application.Finances.Abstractions;
using Kodvian.Core.Application.Finances.Dtos;
using Kodvian.Core.Application.Finances.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kodvian.Core.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/financial-movements")]
public class FinancialMovementsController : ControllerBase
{
    private readonly IFinancialMovementService _financialMovementService;

    public FinancialMovementsController(IFinancialMovementService financialMovementService)
    {
        _financialMovementService = financialMovementService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<PagedResultDto<FinancialMovementListItemDto>>>> GetPaged([FromQuery] FinancialMovementListRequestDto request, CancellationToken cancellationToken)
    {
        if (request.DateFrom.HasValue && request.DateTo.HasValue && request.DateFrom > request.DateTo)
        {
            return BadRequest(ApiResponseDto<PagedResultDto<FinancialMovementListItemDto>>.Fail("El rango de fechas es inválido"));
        }

        var data = await _financialMovementService.GetPagedAsync(request, cancellationToken);
        return Ok(ApiResponseDto<PagedResultDto<FinancialMovementListItemDto>>.Ok(data, "Movimientos financieros obtenidos correctamente"));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponseDto<FinancialMovementDetailDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var data = await _financialMovementService.GetByIdAsync(id, cancellationToken);
        if (data is null)
        {
            return NotFound(ApiResponseDto<FinancialMovementDetailDto>.Fail("Movimiento no encontrado"));
        }

        return Ok(ApiResponseDto<FinancialMovementDetailDto>.Ok(data, "Detalle del movimiento obtenido correctamente"));
    }

    [HttpGet("monthly-summary")]
    public async Task<ActionResult<ApiResponseDto<FinanceMonthlySummaryDto>>> GetMonthlySummary([FromQuery] int? year, [FromQuery] int? month, CancellationToken cancellationToken)
    {
        if (year.HasValue && (year.Value < 2000 || year.Value > 2100))
        {
            return BadRequest(ApiResponseDto<FinanceMonthlySummaryDto>.Fail("El año debe estar entre 2000 y 2100"));
        }

        if (month.HasValue && (month.Value < 1 || month.Value > 12))
        {
            return BadRequest(ApiResponseDto<FinanceMonthlySummaryDto>.Fail("El mes debe estar entre 1 y 12"));
        }

        var data = await _financialMovementService.GetMonthlySummaryAsync(year, month, cancellationToken);
        return Ok(ApiResponseDto<FinanceMonthlySummaryDto>.Ok(data, "Resumen mensual obtenido correctamente"));
    }

    [HttpGet("lookups")]
    public async Task<ActionResult<ApiResponseDto<FinanceLookupsDto>>> GetLookups(CancellationToken cancellationToken)
    {
        var data = await _financialMovementService.GetLookupsAsync(cancellationToken);
        return Ok(ApiResponseDto<FinanceLookupsDto>.Ok(data, "Datos de referencia obtenidos correctamente"));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<FinancialMovementDetailDto>>> Create([FromBody] FinancialMovementUpsertRequestDto request, CancellationToken cancellationToken)
    {
        var validationError = RequestValidation.Validate(request);
        if (validationError is not null)
        {
            return BadRequest(ApiResponseDto<FinancialMovementDetailDto>.Fail(validationError));
        }

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var createdById))
        {
            return Unauthorized(ApiResponseDto<FinancialMovementDetailDto>.Fail("Token inválido"));
        }

        var data = await _financialMovementService.CreateAsync(createdById, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = data.Id }, ApiResponseDto<FinancialMovementDetailDto>.Ok(data, "El movimiento se registró correctamente"));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponseDto<FinancialMovementDetailDto>>> Update(Guid id, [FromBody] FinancialMovementUpsertRequestDto request, CancellationToken cancellationToken)
    {
        var validationError = RequestValidation.Validate(request);
        if (validationError is not null)
        {
            return BadRequest(ApiResponseDto<FinancialMovementDetailDto>.Fail(validationError));
        }

        var data = await _financialMovementService.UpdateAsync(id, request, cancellationToken);
        if (data is null)
        {
            return NotFound(ApiResponseDto<FinancialMovementDetailDto>.Fail("Movimiento no encontrado"));
        }

        return Ok(ApiResponseDto<FinancialMovementDetailDto>.Ok(data, "El movimiento se actualizó correctamente"));
    }

}
