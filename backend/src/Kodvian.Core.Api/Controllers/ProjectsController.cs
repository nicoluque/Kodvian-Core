using Kodvian.Core.Application.Common.Models;
using Kodvian.Core.Application.Projects.Abstractions;
using Kodvian.Core.Application.Projects.Dtos;
using Kodvian.Core.Application.Projects.Requests;
using Kodvian.Core.Api.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kodvian.Core.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/projects")]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectsController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<PagedResultDto<ProjectListItemDto>>>> GetPaged(
        [FromQuery] ProjectListRequestDto request,
        CancellationToken cancellationToken)
    {
        var data = await _projectService.GetPagedAsync(request, cancellationToken);
        return Ok(ApiResponseDto<PagedResultDto<ProjectListItemDto>>.Ok(data, "Proyectos obtenidos correctamente"));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponseDto<ProjectDetailDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var data = await _projectService.GetByIdAsync(id, cancellationToken);
        if (data is null)
        {
            return NotFound(ApiResponseDto<ProjectDetailDto>.Fail("Proyecto no encontrado"));
        }

        return Ok(ApiResponseDto<ProjectDetailDto>.Ok(data, "Detalle del proyecto obtenido correctamente"));
    }

    [HttpGet("lookups")]
    public async Task<ActionResult<ApiResponseDto<ProjectLookupsDto>>> GetLookups(CancellationToken cancellationToken)
    {
        var data = await _projectService.GetLookupsAsync(cancellationToken);
        return Ok(ApiResponseDto<ProjectLookupsDto>.Ok(data, "Datos de referencia obtenidos correctamente"));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<ProjectDetailDto>>> Create([FromBody] ProjectUpsertRequestDto request, CancellationToken cancellationToken)
    {
        var validationError = RequestValidation.Validate(request);
        if (validationError is not null)
        {
            return BadRequest(ApiResponseDto<ProjectDetailDto>.Fail(validationError));
        }

        var data = await _projectService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = data.Id }, ApiResponseDto<ProjectDetailDto>.Ok(data, "El proyecto se creó correctamente"));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponseDto<ProjectDetailDto>>> Update(Guid id, [FromBody] ProjectUpsertRequestDto request, CancellationToken cancellationToken)
    {
        var validationError = RequestValidation.Validate(request);
        if (validationError is not null)
        {
            return BadRequest(ApiResponseDto<ProjectDetailDto>.Fail(validationError));
        }

        var data = await _projectService.UpdateAsync(id, request, cancellationToken);
        if (data is null)
        {
            return NotFound(ApiResponseDto<ProjectDetailDto>.Fail("Proyecto no encontrado"));
        }

        return Ok(ApiResponseDto<ProjectDetailDto>.Ok(data, "El proyecto se actualizó correctamente"));
    }

}
