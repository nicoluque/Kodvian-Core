using Kodvian.Core.Api.Validation;
using Kodvian.Core.Application.Common.Models;
using Kodvian.Core.Application.Developers.Abstractions;
using Kodvian.Core.Application.Developers.Dtos;
using Kodvian.Core.Application.Developers.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kodvian.Core.Api.Controllers;

[ApiController]
[Authorize(Policy = "ProjectsRead")]
[Route("api")]
public class ProjectDeveloperAssignmentsController : ControllerBase
{
    private readonly IProjectDeveloperAssignmentService _assignmentService;

    public ProjectDeveloperAssignmentsController(IProjectDeveloperAssignmentService assignmentService)
    {
        _assignmentService = assignmentService;
    }

    [HttpGet("projects/{projectId:guid}/developer-assignments")]
    public async Task<ActionResult<ApiResponseDto<IReadOnlyCollection<ProjectDeveloperAssignmentDto>>>> GetByProject(Guid projectId, CancellationToken cancellationToken)
    {
        var data = await _assignmentService.GetByProjectAsync(projectId, cancellationToken);
        return Ok(ApiResponseDto<IReadOnlyCollection<ProjectDeveloperAssignmentDto>>.Ok(data, "Equipo asignado obtenido correctamente"));
    }

    [HttpPost("projects/{projectId:guid}/developer-assignments")]
    [Authorize(Policy = "ProjectsWrite")]
    public async Task<ActionResult<ApiResponseDto<ProjectDeveloperAssignmentDto>>> Create(Guid projectId, [FromBody] ProjectDeveloperAssignmentCreateRequestDto request, CancellationToken cancellationToken)
    {
        var validationError = RequestValidation.Validate(request);
        if (validationError is not null)
        {
            return BadRequest(ApiResponseDto<ProjectDeveloperAssignmentDto>.Fail(validationError));
        }

        try
        {
            var data = await _assignmentService.CreateAsync(projectId, request, cancellationToken);
            return Ok(ApiResponseDto<ProjectDeveloperAssignmentDto>.Ok(data, "Desarrollador asignado correctamente"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponseDto<ProjectDeveloperAssignmentDto>.Fail(ex.Message));
        }
    }

    [HttpDelete("project-developer-assignments/{id:guid}")]
    [Authorize(Policy = "ProjectsWrite")]
    public async Task<ActionResult<ApiResponseDto<object>>> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _assignmentService.DeactivateAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound(ApiResponseDto<object>.Fail("Asignación no encontrada"));
        }

        return Ok(ApiResponseDto<object>.Ok(new { }, "Asignación eliminada correctamente"));
    }
}
