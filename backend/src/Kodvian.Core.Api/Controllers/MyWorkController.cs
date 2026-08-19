using System.Security.Claims;
using Kodvian.Core.Api.Validation;
using Kodvian.Core.Application.Common.Models;
using Kodvian.Core.Application.Common.Security;
using Kodvian.Core.Application.MyWork.Abstractions;
using Kodvian.Core.Application.MyWork.Dtos;
using Kodvian.Core.Application.Projects.Dtos;
using Kodvian.Core.Application.Projects.Requests;
using Kodvian.Core.Application.Tasks.Dtos;
using Kodvian.Core.Application.Tasks.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kodvian.Core.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/my-work")]
public class MyWorkController : ControllerBase
{
    private readonly IMyWorkService _myWorkService;

    public MyWorkController(IMyWorkService myWorkService)
    {
        _myWorkService = myWorkService;
    }

    [HttpGet("overview")]
    [Authorize(Policy = "DeveloperWorkRead")]
    public async Task<ActionResult<ApiResponseDto<MyWorkOverviewDto>>> GetOverview(CancellationToken cancellationToken)
    {
        if (!TryGetDeveloperId(out var developerId))
        {
            return Forbid();
        }

        var data = await _myWorkService.GetOverviewAsync(developerId, cancellationToken);
        return Ok(ApiResponseDto<MyWorkOverviewDto>.Ok(data, "Trabajo asignado obtenido correctamente"));
    }

    [HttpGet("projects")]
    [Authorize(Policy = "DeveloperWorkRead")]
    public async Task<ActionResult<ApiResponseDto<PagedResultDto<ProjectListItemDto>>>> GetProjects([FromQuery] ProjectListRequestDto request, CancellationToken cancellationToken)
    {
        if (!TryGetDeveloperId(out var developerId))
        {
            return Forbid();
        }

        var data = await _myWorkService.GetProjectsAsync(developerId, request, cancellationToken);
        return Ok(ApiResponseDto<PagedResultDto<ProjectListItemDto>>.Ok(data, "Proyectos asignados obtenidos correctamente"));
    }

    [HttpGet("tasks")]
    [Authorize(Policy = "DeveloperWorkRead")]
    public async Task<ActionResult<ApiResponseDto<PagedResultDto<TaskListItemDto>>>> GetTasks([FromQuery] TaskListRequestDto request, CancellationToken cancellationToken)
    {
        if (!TryGetDeveloperId(out var developerId))
        {
            return Forbid();
        }

        if (request.DueDateFrom.HasValue && request.DueDateTo.HasValue && request.DueDateFrom > request.DueDateTo)
        {
            return BadRequest(ApiResponseDto<PagedResultDto<TaskListItemDto>>.Fail("El rango de vencimiento es inválido"));
        }

        var data = await _myWorkService.GetTasksAsync(developerId, request, cancellationToken);
        return Ok(ApiResponseDto<PagedResultDto<TaskListItemDto>>.Ok(data, "Tareas asignadas obtenidas correctamente"));
    }

    [HttpGet("tasks/kanban")]
    [Authorize(Policy = "DeveloperWorkRead")]
    public async Task<ActionResult<ApiResponseDto<IReadOnlyCollection<TaskKanbanColumnDto>>>> GetTaskKanban([FromQuery] TaskListRequestDto request, CancellationToken cancellationToken)
    {
        if (!TryGetDeveloperId(out var developerId))
        {
            return Forbid();
        }

        var data = await _myWorkService.GetTaskKanbanAsync(developerId, request, cancellationToken);
        return Ok(ApiResponseDto<IReadOnlyCollection<TaskKanbanColumnDto>>.Ok(data, "Tablero de tareas asignadas obtenido correctamente"));
    }

    [HttpGet("tasks/{id:guid}")]
    [Authorize(Policy = "DeveloperWorkRead")]
    public async Task<ActionResult<ApiResponseDto<TaskDetailDto>>> GetTaskById(Guid id, CancellationToken cancellationToken)
    {
        if (!TryGetDeveloperId(out var developerId))
        {
            return Forbid();
        }

        var data = await _myWorkService.GetTaskByIdAsync(developerId, id, cancellationToken);
        if (data is null)
        {
            return NotFound(ApiResponseDto<TaskDetailDto>.Fail("Tarea no encontrada"));
        }

        return Ok(ApiResponseDto<TaskDetailDto>.Ok(data, "Detalle de tarea obtenido correctamente"));
    }

    [HttpPatch("tasks/{id:guid}/status")]
    [Authorize(Policy = "DeveloperTasksStatusWrite")]
    public async Task<ActionResult<ApiResponseDto<TaskDetailDto>>> UpdateTaskStatus(Guid id, [FromBody] TaskStatusUpdateRequestDto request, CancellationToken cancellationToken)
    {
        if (!TryGetDeveloperId(out var developerId))
        {
            return Forbid();
        }

        var validationError = RequestValidation.Validate(request);
        if (validationError is not null)
        {
            return BadRequest(ApiResponseDto<TaskDetailDto>.Fail(validationError));
        }

        var data = await _myWorkService.UpdateTaskStatusAsync(developerId, id, request, cancellationToken);
        if (data is null)
        {
            return NotFound(ApiResponseDto<TaskDetailDto>.Fail("Tarea no encontrada"));
        }

        return Ok(ApiResponseDto<TaskDetailDto>.Ok(data, "El estado de la tarea se actualizó correctamente"));
    }

    private bool TryGetDeveloperId(out Guid developerId)
    {
        var claimValue = User.FindFirstValue(CustomClaimTypes.DeveloperId);
        return Guid.TryParse(claimValue, out developerId);
    }
}
