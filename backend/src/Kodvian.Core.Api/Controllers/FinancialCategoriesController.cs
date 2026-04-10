using Kodvian.Core.Application.Common.Models;
using Kodvian.Core.Application.Finances.Abstractions;
using Kodvian.Core.Application.Finances.Dtos;
using Kodvian.Core.Application.Finances.Requests;
using Kodvian.Core.Api.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kodvian.Core.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/financial-categories")]
public class FinancialCategoriesController : ControllerBase
{
    private readonly IFinancialCategoryService _financialCategoryService;

    public FinancialCategoriesController(IFinancialCategoryService financialCategoryService)
    {
        _financialCategoryService = financialCategoryService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<IReadOnlyCollection<FinancialCategoryDto>>>> GetAll(CancellationToken cancellationToken)
    {
        var data = await _financialCategoryService.GetAllAsync(cancellationToken);
        return Ok(ApiResponseDto<IReadOnlyCollection<FinancialCategoryDto>>.Ok(data, "Categorías obtenidas correctamente"));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<FinancialCategoryDto>>> Create([FromBody] FinancialCategoryUpsertRequestDto request, CancellationToken cancellationToken)
    {
        var validationError = RequestValidation.Validate(request);
        if (validationError is not null)
        {
            return BadRequest(ApiResponseDto<FinancialCategoryDto>.Fail(validationError));
        }

        var data = await _financialCategoryService.CreateAsync(request, cancellationToken);
        return Ok(ApiResponseDto<FinancialCategoryDto>.Ok(data, "La categoría se creó correctamente"));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponseDto<FinancialCategoryDto>>> Update(Guid id, [FromBody] FinancialCategoryUpsertRequestDto request, CancellationToken cancellationToken)
    {
        var validationError = RequestValidation.Validate(request);
        if (validationError is not null)
        {
            return BadRequest(ApiResponseDto<FinancialCategoryDto>.Fail(validationError));
        }

        var data = await _financialCategoryService.UpdateAsync(id, request, cancellationToken);
        if (data is null)
        {
            return NotFound(ApiResponseDto<FinancialCategoryDto>.Fail("Categoría no encontrada"));
        }

        return Ok(ApiResponseDto<FinancialCategoryDto>.Ok(data, "La categoría se actualizó correctamente"));
    }
}
