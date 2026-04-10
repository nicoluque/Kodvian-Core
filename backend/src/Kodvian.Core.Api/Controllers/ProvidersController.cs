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
[Route("api/providers")]
public class ProvidersController : ControllerBase
{
    private readonly IProviderService _providerService;

    public ProvidersController(IProviderService providerService)
    {
        _providerService = providerService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<IReadOnlyCollection<ProviderDto>>>> GetAll(CancellationToken cancellationToken)
    {
        var data = await _providerService.GetAllAsync(cancellationToken);
        return Ok(ApiResponseDto<IReadOnlyCollection<ProviderDto>>.Ok(data, "Proveedores obtenidos correctamente"));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<ProviderDto>>> Create([FromBody] ProviderUpsertRequestDto request, CancellationToken cancellationToken)
    {
        var validationError = RequestValidation.Validate(request);
        if (validationError is not null)
        {
            return BadRequest(ApiResponseDto<ProviderDto>.Fail(validationError));
        }

        var data = await _providerService.CreateAsync(request, cancellationToken);
        return Ok(ApiResponseDto<ProviderDto>.Ok(data, "El proveedor se creó correctamente"));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponseDto<ProviderDto>>> Update(Guid id, [FromBody] ProviderUpsertRequestDto request, CancellationToken cancellationToken)
    {
        var validationError = RequestValidation.Validate(request);
        if (validationError is not null)
        {
            return BadRequest(ApiResponseDto<ProviderDto>.Fail(validationError));
        }

        var data = await _providerService.UpdateAsync(id, request, cancellationToken);
        if (data is null)
        {
            return NotFound(ApiResponseDto<ProviderDto>.Fail("Proveedor no encontrado"));
        }

        return Ok(ApiResponseDto<ProviderDto>.Ok(data, "El proveedor se actualizó correctamente"));
    }
}
