using Kodvian.Core.Application.Common.Models;
using Kodvian.Core.Application.MyWork.Abstractions;
using Kodvian.Core.Application.MyWork.Dtos;
using Kodvian.Core.Application.Projects.Dtos;
using Kodvian.Core.Application.Projects.Requests;
using Kodvian.Core.Application.Tasks.Dtos;
using Kodvian.Core.Application.Tasks.Requests;
using Kodvian.Core.Domain.Entities;
using Kodvian.Core.Domain.Enums;
using Kodvian.Core.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using DomainTaskStatus = Kodvian.Core.Domain.Enums.TaskStatus;

namespace Kodvian.Core.Infrastructure.Services;

public class MyWorkService : IMyWorkService
{
    private readonly KodvianDbContext _dbContext;

    public MyWorkService(KodvianDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<MyWorkOverviewDto> GetOverviewAsync(Guid developerId, CancellationToken cancellationToken = default)
    {
        var projects = await BuildProjectsQuery(developerId, new ProjectListRequestDto { PageSize = 6 })
            .OrderByDescending(x => x.FechaCreacion)
            .Take(6)
            .Select(ToProjectListItem())
            .ToListAsync(cancellationToken);

        var tasks = await BuildTasksQuery(developerId, new TaskListRequestDto { PageSize = 8 })
            .OrderBy(x => x.Estado)
            .ThenBy(x => x.OrdenKanban)
            .ThenByDescending(x => x.FechaCreacion)
            .Take(8)
            .Select(ToTaskListItem())
            .ToListAsync(cancellationToken);

        return new MyWorkOverviewDto
        {
            Projects = projects,
            Tasks = tasks
        };
    }

    public async Task<PagedResultDto<ProjectListItemDto>> GetProjectsAsync(Guid developerId, ProjectListRequestDto request, CancellationToken cancellationToken = default)
    {
        var query = BuildProjectsQuery(developerId, request);
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.FechaCreacion)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(ToProjectListItem())
            .ToListAsync(cancellationToken);

        return new PagedResultDto<ProjectListItemDto>
        {
            Items = items,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<PagedResultDto<TaskListItemDto>> GetTasksAsync(Guid developerId, TaskListRequestDto request, CancellationToken cancellationToken = default)
    {
        var query = BuildTasksQuery(developerId, request);
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(x => x.Estado)
            .ThenBy(x => x.OrdenKanban)
            .ThenByDescending(x => x.FechaCreacion)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(ToTaskListItem())
            .ToListAsync(cancellationToken);

        return new PagedResultDto<TaskListItemDto>
        {
            Items = items,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<IReadOnlyCollection<TaskKanbanColumnDto>> GetTaskKanbanAsync(Guid developerId, TaskListRequestDto request, CancellationToken cancellationToken = default)
    {
        var items = await BuildTasksQuery(developerId, request)
            .OrderBy(x => x.Estado)
            .ThenBy(x => x.OrdenKanban)
            .Select(x => new
            {
                x.Id,
                x.Titulo,
                ProjectName = x.Proyecto != null ? x.Proyecto.Nombre : string.Empty,
                DeveloperName = x.Developer != null ? x.Developer.FullName : null,
                ResponsibleName = x.Responsable != null ? x.Responsable.FullName : null,
                Priority = x.Prioridad,
                DueDate = x.FechaVencimiento,
                Status = x.Estado,
                x.OrdenKanban
            })
            .ToListAsync(cancellationToken);

        var statusOrder = new[] { "Pendiente", "EnCurso", "Bloqueada", "Finalizada", "Cancelada" };
        return statusOrder
            .Select(status => new TaskKanbanColumnDto
            {
                Status = status,
                Title = ToUiStatus(status),
                Items = items
                    .Where(x => x.Status.ToString() == status)
                    .Select(x => new TaskKanbanItemDto
                    {
                        Id = x.Id,
                        Title = x.Titulo,
                        ProjectName = x.ProjectName,
                        DeveloperName = x.DeveloperName,
                        ResponsibleName = x.ResponsibleName,
                        Priority = x.Priority.ToString(),
                        DueDate = x.DueDate,
                        KanbanOrder = x.OrdenKanban
                    })
                    .ToList()
            })
            .ToList();
    }

    public async Task<TaskDetailDto?> GetTaskByIdAsync(Guid developerId, Guid taskId, CancellationToken cancellationToken = default)
    {
        return await BuildTasksQuery(developerId, new TaskListRequestDto())
            .Where(x => x.Id == taskId)
            .Select(ToTaskDetail())
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<TaskDetailDto?> UpdateTaskStatusAsync(Guid developerId, Guid taskId, TaskStatusUpdateRequestDto request, CancellationToken cancellationToken = default)
    {
        var task = await _dbContext.Tasks.FirstOrDefaultAsync(x => x.Id == taskId && x.DeveloperId == developerId, cancellationToken);
        if (task is null)
        {
            return null;
        }

        task.Estado = ParseTaskStatus(request.Status);
        task.OrdenKanban = request.KanbanOrder;
        task.FechaActualizacion = DateTime.UtcNow;
        if (task.Estado == DomainTaskStatus.Finalizada)
        {
            task.FechaFinalizacion ??= DateOnly.FromDateTime(DateTime.UtcNow);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return await GetTaskByIdAsync(developerId, taskId, cancellationToken);
    }

    private IQueryable<Project> BuildProjectsQuery(Guid developerId, ProjectListRequestDto request)
    {
        var query = _dbContext.Projects
            .AsNoTracking()
            .Where(x => x.Activo
                && (x.DeveloperContracts.Any(c => c.DeveloperId == developerId && c.Activo)
                    || x.Tareas.Any(t => t.DeveloperId == developerId && t.Activo)));

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = $"%{request.Search.Trim()}%";
            query = query.Where(x => EF.Functions.ILike(x.Nombre, search));
        }

        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<ProjectStatus>(request.Status, true, out var status))
        {
            query = query.Where(x => x.Estado == status);
        }

        if (!string.IsNullOrWhiteSpace(request.Priority) && Enum.TryParse<ProjectPriority>(request.Priority, true, out var priority))
        {
            query = query.Where(x => x.Prioridad == priority);
        }

        return query;
    }

    private IQueryable<TaskItem> BuildTasksQuery(Guid developerId, TaskListRequestDto request)
    {
        var query = _dbContext.Tasks
            .AsNoTracking()
            .Where(x => x.DeveloperId == developerId && x.Activo);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = $"%{request.Search.Trim()}%";
            query = query.Where(x => EF.Functions.ILike(x.Titulo, search));
        }

        if (request.ProjectId.HasValue)
        {
            query = query.Where(x => x.ProyectoId == request.ProjectId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<DomainTaskStatus>(request.Status, true, out var status))
        {
            query = query.Where(x => x.Estado == status);
        }

        if (!string.IsNullOrWhiteSpace(request.Priority) && Enum.TryParse<TaskPriority>(request.Priority, true, out var priority))
        {
            query = query.Where(x => x.Prioridad == priority);
        }

        if (request.DueDateFrom.HasValue)
        {
            query = query.Where(x => x.FechaVencimiento >= request.DueDateFrom.Value);
        }

        if (request.DueDateTo.HasValue)
        {
            query = query.Where(x => x.FechaVencimiento <= request.DueDateTo.Value);
        }

        return query;
    }

    private static System.Linq.Expressions.Expression<Func<Project, ProjectListItemDto>> ToProjectListItem()
    {
        return x => new ProjectListItemDto
        {
            Id = x.Id,
            Name = x.Nombre,
            ClientId = x.ClienteId,
            ClientName = x.Cliente != null ? x.Cliente.CommercialName : string.Empty,
            ResponsibleId = x.ResponsableId,
            ResponsibleName = x.Responsable != null ? x.Responsable.FullName : null,
            Status = x.Estado.ToString(),
            Priority = x.Prioridad.ToString(),
            StartDate = x.FechaInicio,
            EstimatedDeliveryDate = x.FechaEntregaEstimada,
            ProgressPercentage = x.PorcentajeAvance,
            IsActive = x.Activo
        };
    }

    private static System.Linq.Expressions.Expression<Func<TaskItem, TaskListItemDto>> ToTaskListItem()
    {
        return x => new TaskListItemDto
        {
            Id = x.Id,
            ProjectId = x.ProyectoId,
            ProjectName = x.Proyecto != null ? x.Proyecto.Nombre : string.Empty,
            Title = x.Titulo,
            DeveloperId = x.DeveloperId,
            DeveloperName = x.Developer != null ? x.Developer.FullName : null,
            ResponsibleId = x.ResponsableId,
            ResponsibleName = x.Responsable != null ? x.Responsable.FullName : null,
            Status = x.Estado.ToString(),
            Priority = x.Prioridad.ToString(),
            DueDate = x.FechaVencimiento,
            EstimatedHours = x.HorasEstimadas,
            RealHours = x.HorasReales,
            KanbanOrder = x.OrdenKanban,
            IsActive = x.Activo
        };
    }

    private static System.Linq.Expressions.Expression<Func<TaskItem, TaskDetailDto>> ToTaskDetail()
    {
        return x => new TaskDetailDto
        {
            Id = x.Id,
            ProjectId = x.ProyectoId,
            ProjectName = x.Proyecto != null ? x.Proyecto.Nombre : string.Empty,
            Title = x.Titulo,
            Description = x.Descripcion,
            DeveloperId = x.DeveloperId,
            DeveloperName = x.Developer != null ? x.Developer.FullName : null,
            ResponsibleId = x.ResponsableId,
            ResponsibleName = x.Responsable != null ? x.Responsable.FullName : null,
            CreatedById = x.CreadoPorId,
            CreatedByName = x.CreadoPor != null ? x.CreadoPor.FullName : string.Empty,
            Status = x.Estado.ToString(),
            Priority = x.Prioridad.ToString(),
            StartDate = x.FechaInicio,
            DueDate = x.FechaVencimiento,
            FinishedDate = x.FechaFinalizacion,
            EstimatedHours = x.HorasEstimadas,
            RealHours = x.HorasReales,
            KanbanOrder = x.OrdenKanban,
            IsActive = x.Activo,
            CreatedAt = x.FechaCreacion,
            UpdatedAt = x.FechaActualizacion
        };
    }

    private static DomainTaskStatus ParseTaskStatus(string status)
    {
        return Enum.TryParse<DomainTaskStatus>(status, true, out var parsed) ? parsed : throw new InvalidOperationException("El estado de la tarea no es válido.");
    }

    private static string ToUiStatus(string status)
    {
        return status switch
        {
            "EnCurso" => "En curso",
            _ => status
        };
    }
}
