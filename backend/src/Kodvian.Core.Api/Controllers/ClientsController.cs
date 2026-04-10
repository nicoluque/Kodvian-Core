using Kodvian.Core.Application.Clients.Abstractions;
using Kodvian.Core.Application.Clients.Dtos;
using Kodvian.Core.Application.Clients.Requests;
using Kodvian.Core.Application.Common.Models;
using Kodvian.Core.Api.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kodvian.Core.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/clients")]
public class ClientsController : ControllerBase
{
    private readonly IClientService _clientService;

    public ClientsController(IClientService clientService)
    {
        _clientService = clientService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<PagedResultDto<ClientListItemDto>>>> GetPaged(
        [FromQuery] ClientListRequestDto request,
        CancellationToken cancellationToken)
    {
        var data = await _clientService.GetPagedAsync(request, cancellationToken);
        return Ok(ApiResponseDto<PagedResultDto<ClientListItemDto>>.Ok(data, "Clientes obtenidos correctamente"));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponseDto<ClientDetailDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var data = await _clientService.GetByIdAsync(id, cancellationToken);
        if (data is null)
        {
            return NotFound(ApiResponseDto<ClientDetailDto>.Fail("Cliente no encontrado"));
        }

        return Ok(ApiResponseDto<ClientDetailDto>.Ok(data, "Detalle de cliente obtenido correctamente"));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<ClientDetailDto>>> Create(
        [FromBody] ClientUpsertRequestDto request,
        CancellationToken cancellationToken)
    {
        var validationError = RequestValidation.Validate(request);
        if (validationError is not null)
        {
            return BadRequest(ApiResponseDto<ClientDetailDto>.Fail(validationError));
        }

        var data = await _clientService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = data.Id }, ApiResponseDto<ClientDetailDto>.Ok(data, "El cliente se creó correctamente"));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponseDto<ClientDetailDto>>> Update(
        Guid id,
        [FromBody] ClientUpsertRequestDto request,
        CancellationToken cancellationToken)
    {
        var validationError = RequestValidation.Validate(request);
        if (validationError is not null)
        {
            return BadRequest(ApiResponseDto<ClientDetailDto>.Fail(validationError));
        }

        var data = await _clientService.UpdateAsync(id, request, cancellationToken);
        if (data is null)
        {
            return NotFound(ApiResponseDto<ClientDetailDto>.Fail("Cliente no encontrado"));
        }

        return Ok(ApiResponseDto<ClientDetailDto>.Ok(data, "El cliente se actualizó correctamente"));
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<ApiResponseDto<ClientDetailDto>>> ChangeStatus(
        Guid id,
        [FromBody] ChangeClientStatusRequestDto request,
        CancellationToken cancellationToken)
    {
        var validationError = RequestValidation.Validate(request);
        if (validationError is not null)
        {
            return BadRequest(ApiResponseDto<ClientDetailDto>.Fail(validationError));
        }

        var data = await _clientService.ChangeStatusAsync(id, request, cancellationToken);
        if (data is null)
        {
            return NotFound(ApiResponseDto<ClientDetailDto>.Fail("Cliente no encontrado"));
        }

        return Ok(ApiResponseDto<ClientDetailDto>.Ok(data, "El estado del cliente se actualizó correctamente"));
    }

}
