using Kodvian.Core.Application.Common.Models;
using Kodvian.Core.Application.Integrations.GitHub.Abstractions;
using Kodvian.Core.Application.Integrations.GitHub.Dtos;
using Kodvian.Core.Application.MyWork.Abstractions;
using Kodvian.Core.Application.MyWork.Dtos;
using Kodvian.Core.Application.MyWork.Requests;
using Kodvian.Core.Application.Projects.Dtos;
using Kodvian.Core.Application.Projects.Requests;
using Kodvian.Core.Application.Tasks.Dtos;
using Kodvian.Core.Application.Tasks.Requests;
using Kodvian.Core.Domain.Entities;
using Kodvian.Core.Domain.Enums;
using Kodvian.Core.Infrastructure.Integrations.GitHub;
using Kodvian.Core.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using DomainTaskStatus = Kodvian.Core.Domain.Enums.TaskStatus;

namespace Kodvian.Core.Infrastructure.Services;

public class MyWorkService : IMyWorkService
{
    private readonly KodvianDbContext _dbContext;
    private readonly IGitHubApiService _gitHubApiService;
    private readonly IGitHubTokenProvider _gitHubTokenProvider;
    private readonly GitHubOptions _gitHubOptions;

    public MyWorkService(
        KodvianDbContext dbContext,
        IGitHubApiService gitHubApiService,
        IGitHubTokenProvider gitHubTokenProvider,
        IOptions<GitHubOptions> gitHubOptions)
    {
        _dbContext = dbContext;
        _gitHubApiService = gitHubApiService;
        _gitHubTokenProvider = gitHubTokenProvider;
        _gitHubOptions = gitHubOptions.Value;
    }

    public async Task<MyWorkOverviewDto> GetOverviewAsync(Guid developerId, Guid userId, CancellationToken cancellationToken = default)
    {
        var githubNotConnected = await IsGitHubNotConnectedAsync(userId, cancellationToken);

        var repositoriesQuery = BuildAssignedRepositoriesQuery(developerId, new MyWorkRepositoryListRequestDto());
        var repositoryCount = await repositoriesQuery.CountAsync(cancellationToken);
        var repositories = await repositoriesQuery
            .OrderBy(x => x.GitHubOwner)
            .ThenBy(x => x.GitHubRepoName)
            .Take(6)
            .Select(x => new MyWorkRepositoryListItemDto
            {
                ProjectId = x.Id,
                ProjectName = x.Nombre,
                ClientName = x.Cliente != null ? x.Cliente.CommercialName : string.Empty,
                ProjectStatus = x.Estado.ToString(),
                GitHubOwner = x.GitHubOwner!,
                GitHubRepoName = x.GitHubRepoName!,
                GitHubRepoUrl = x.GitHubRepoUrl,
                GitHubRepoId = x.GitHubRepoId,
                OpenIssuesCount = x.GitHubIssueLinks.Count(i => i.Activo && i.Status == GitHubIssueStatus.Open)
            })
            .ToListAsync(cancellationToken);

        var issuesQuery = BuildIssuesQuery(developerId, new MyWorkIssueListRequestDto());
        var totalIssuesCount = await issuesQuery.CountAsync(cancellationToken);
        var openIssuesCount = await issuesQuery.CountAsync(x => x.Status == GitHubIssueStatus.Open, cancellationToken);
        var issues = await issuesQuery
            .OrderByDescending(x => x.FechaCreacion)
            .Take(8)
            .Select(ToIssueListItem())
            .ToListAsync(cancellationToken);

        return new MyWorkOverviewDto
        {
            GitHubNotConnected = githubNotConnected,
            RepositoryCount = repositoryCount,
            TotalIssuesCount = totalIssuesCount,
            OpenIssuesCount = openIssuesCount,
            Repositories = repositories,
            Issues = issues
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

    public async Task<MyWorkRepositoriesPageDto> GetAssignedRepositoriesAsync(
        Guid developerId,
        Guid userId,
        MyWorkRepositoryListRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var githubNotConnected = await IsGitHubNotConnectedAsync(userId, cancellationToken);

        var query = BuildAssignedRepositoriesQuery(developerId, request);
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(x => x.GitHubOwner)
            .ThenBy(x => x.GitHubRepoName)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new MyWorkRepositoryListItemDto
            {
                ProjectId = x.Id,
                ProjectName = x.Nombre,
                ClientName = x.Cliente != null ? x.Cliente.CommercialName : string.Empty,
                ProjectStatus = x.Estado.ToString(),
                GitHubOwner = x.GitHubOwner!,
                GitHubRepoName = x.GitHubRepoName!,
                GitHubRepoUrl = x.GitHubRepoUrl,
                GitHubRepoId = x.GitHubRepoId,
                OpenIssuesCount = x.GitHubIssueLinks.Count(i => i.Activo && i.Status == GitHubIssueStatus.Open)
            })
            .ToListAsync(cancellationToken);

        return new MyWorkRepositoriesPageDto
        {
            GitHubNotConnected = githubNotConnected,
            Items = items,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<MyWorkIssueListItemDto> CreateIssueAsync(
        Guid developerId,
        Guid userId,
        CreateMyWorkIssueRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var project = await BuildAccessibleProjectsQuery(developerId)
            .Where(x => x.Id == request.ProjectId && x.GitHubOwner != null && x.GitHubRepoName != null)
            .Select(x => new
            {
                x.Id,
                x.Nombre,
                Owner = x.GitHubOwner!,
                Repo = x.GitHubRepoName!
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (project is null)
        {
            throw new InvalidOperationException("No tenés acceso a ese proyecto o no tiene repositorio GitHub configurado.");
        }

        var user = await _dbContext.Users.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == userId && x.Activo, cancellationToken)
            ?? throw new ArgumentException("Usuario no encontrado.");

        if (string.IsNullOrWhiteSpace(user.GitHubUsername))
        {
            throw new InvalidOperationException(GitHubTokenProvider.ReconnectMessage);
        }

        var token = await _gitHubTokenProvider.GetValidTokenAsync(userId, cancellationToken);
        TaskPriority? priority = null;
        if (!string.IsNullOrWhiteSpace(request.Priority) && Enum.TryParse<TaskPriority>(request.Priority, true, out var parsedPriority))
        {
            priority = parsedPriority;
        }

        IReadOnlyList<string>? labels = string.IsNullOrWhiteSpace(_gitHubOptions.DefaultLabel)
            ? null
            : [_gitHubOptions.DefaultLabel];

        var githubIssue = await _gitHubApiService.CreateIssueAsync(
            project.Owner,
            project.Repo,
            new CreateGitHubIssueRequest
            {
                Title = request.Title.Trim(),
                Body = request.Description,
                Assignees = [user.GitHubUsername],
                Labels = labels
            },
            token,
            cancellationToken);

        var link = new GitHubIssueLink
        {
            ProjectId = project.Id,
            DeveloperId = developerId,
            GitHubIssueNumber = githubIssue.Number,
            GitHubIssueNodeId = githubIssue.NodeId,
            GitHubIssueUrl = githubIssue.HtmlUrl,
            Title = githubIssue.Title,
            Description = request.Description,
            Status = GitHubIssueStatus.Open,
            Priority = priority,
            AssignedGitHubUsername = user.GitHubUsername,
            LastSyncedAt = DateTime.UtcNow,
            SyncDirection = SyncDirection.FromKodvian,
            Activo = true
        };

        _dbContext.GitHubIssueLinks.Add(link);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new MyWorkIssueListItemDto
        {
            Id = link.Id,
            ProjectId = link.ProjectId,
            ProjectName = project.Nombre,
            Title = link.Title,
            RepositoryFullName = $"{project.Owner}/{project.Repo}",
            GitHubIssueNumber = link.GitHubIssueNumber,
            GitHubIssueUrl = link.GitHubIssueUrl,
            Status = link.Status.ToString(),
            Priority = link.Priority?.ToString(),
            CreatedAt = link.FechaCreacion
        };
    }

    public async Task<MyWorkIssueListItemDto?> UpdateIssueStatusAsync(
        Guid developerId,
        Guid userId,
        Guid issueLinkId,
        UpdateMyWorkIssueStatusRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var link = await _dbContext.GitHubIssueLinks
            .Include(x => x.Project)
            .FirstOrDefaultAsync(x => x.Id == issueLinkId && x.Activo, cancellationToken);

        if (link is null)
        {
            return null;
        }

        var hasProjectAccess = await BuildAccessibleProjectsQuery(developerId)
            .AnyAsync(
                x => x.Id == link.ProjectId && x.GitHubOwner != null && x.GitHubRepoName != null,
                cancellationToken);

        if (!hasProjectAccess)
        {
            return null;
        }

        var newStatus = ParseGitHubIssueStatus(request.Status);
        if (link.Status == newStatus)
        {
            return MapIssueLinkToDto(link);
        }

        var project = link.Project;
        if (project?.GitHubOwner is null || project.GitHubRepoName is null)
        {
            throw new InvalidOperationException("El proyecto no tiene repositorio GitHub configurado.");
        }

        var user = await _dbContext.Users.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == userId && x.Activo, cancellationToken)
            ?? throw new ArgumentException("Usuario no encontrado.");

        if (string.IsNullOrWhiteSpace(user.GitHubUsername))
        {
            throw new InvalidOperationException(GitHubTokenProvider.ReconnectMessage);
        }

        var token = await _gitHubTokenProvider.GetValidTokenAsync(userId, cancellationToken);
        var githubIssue = await _gitHubApiService.UpdateIssueAsync(
            project.GitHubOwner,
            project.GitHubRepoName,
            link.GitHubIssueNumber,
            new UpdateGitHubIssueRequest
            {
                State = newStatus == GitHubIssueStatus.Closed ? "closed" : "open"
            },
            token,
            cancellationToken);

        link.Status = MapGitHubState(githubIssue.State);
        link.LastSyncedAt = DateTime.UtcNow;
        link.SyncDirection = SyncDirection.FromKodvian;
        link.FechaActualizacion = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapIssueLinkToDto(link);
    }

    public async Task<PagedResultDto<MyWorkIssueListItemDto>> GetIssuesAsync(
        Guid developerId,
        MyWorkIssueListRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var query = BuildIssuesQuery(developerId, request);
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.FechaCreacion)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(ToIssueListItem())
            .ToListAsync(cancellationToken);

        return new PagedResultDto<MyWorkIssueListItemDto>
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

    private IQueryable<Project> BuildAccessibleProjectsQuery(Guid developerId)
    {
        return _dbContext.Projects
            .AsNoTracking()
            .Where(x => x.Activo
                && (x.DeveloperAssignments.Any(a => a.DeveloperId == developerId && a.Activo)
                    || x.DeveloperContracts.Any(c => c.DeveloperId == developerId && c.Activo)
                    || x.Tareas.Any(t => t.DeveloperId == developerId && t.Activo)));
    }

    private IQueryable<Project> BuildProjectsQuery(Guid developerId, ProjectListRequestDto request)
    {
        var query = BuildAccessibleProjectsQuery(developerId);

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

    private async Task<bool> IsGitHubNotConnectedAsync(Guid userId, CancellationToken cancellationToken)
    {
        return !await _dbContext.Users.AsNoTracking()
            .AnyAsync(x => x.Id == userId && x.GitHubConnectedAt != null && x.GitHubAccessTokenEncrypted != null, cancellationToken);
    }

    private IQueryable<GitHubIssueLink> BuildIssuesQuery(Guid developerId, MyWorkIssueListRequestDto request)
    {
        var accessibleProjectIds = BuildAccessibleProjectsQuery(developerId)
            .Where(x => x.GitHubOwner != null && x.GitHubRepoName != null)
            .Select(x => x.Id);

        var query = _dbContext.GitHubIssueLinks
            .AsNoTracking()
            .Where(x => x.Activo && accessibleProjectIds.Contains(x.ProjectId));

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = $"%{request.Search.Trim()}%";
            query = query.Where(x => EF.Functions.ILike(x.Title, search));
        }

        if (request.ProjectId.HasValue)
        {
            query = query.Where(x => x.ProjectId == request.ProjectId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<GitHubIssueStatus>(request.Status, true, out var status))
        {
            query = query.Where(x => x.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(request.Priority) && Enum.TryParse<TaskPriority>(request.Priority, true, out var priority))
        {
            query = query.Where(x => x.Priority == priority);
        }

        return query;
    }

    private IQueryable<Project> BuildAssignedRepositoriesQuery(Guid developerId, MyWorkRepositoryListRequestDto request)
    {
        var query = BuildAccessibleProjectsQuery(developerId)
            .Where(x => x.GitHubOwner != null && x.GitHubRepoName != null);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = $"%{request.Search.Trim()}%";
            query = query.Where(x =>
                EF.Functions.ILike(x.Nombre, search)
                || EF.Functions.ILike(x.GitHubOwner!, search)
                || EF.Functions.ILike(x.GitHubRepoName!, search));
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

    private static System.Linq.Expressions.Expression<Func<GitHubIssueLink, MyWorkIssueListItemDto>> ToIssueListItem()
    {
        return x => new MyWorkIssueListItemDto
        {
            Id = x.Id,
            ProjectId = x.ProjectId,
            ProjectName = x.Project != null ? x.Project.Nombre : string.Empty,
            Title = x.Title,
            RepositoryFullName = x.Project != null && x.Project.GitHubOwner != null && x.Project.GitHubRepoName != null
                ? x.Project.GitHubOwner + "/" + x.Project.GitHubRepoName
                : string.Empty,
            GitHubIssueNumber = x.GitHubIssueNumber,
            GitHubIssueUrl = x.GitHubIssueUrl,
            Status = x.Status.ToString(),
            Priority = x.Priority != null ? x.Priority.ToString() : null,
            CreatedAt = x.FechaCreacion
        };
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

    private static GitHubIssueStatus ParseGitHubIssueStatus(string status)
    {
        return Enum.TryParse<GitHubIssueStatus>(status, true, out var parsed)
            ? parsed
            : throw new InvalidOperationException("El estado de la issue no es válido.");
    }

    private static GitHubIssueStatus MapGitHubState(string state)
    {
        return state.Equals("closed", StringComparison.OrdinalIgnoreCase)
            ? GitHubIssueStatus.Closed
            : GitHubIssueStatus.Open;
    }

    private static MyWorkIssueListItemDto MapIssueLinkToDto(GitHubIssueLink link)
    {
        return new MyWorkIssueListItemDto
        {
            Id = link.Id,
            ProjectId = link.ProjectId,
            ProjectName = link.Project?.Nombre ?? string.Empty,
            Title = link.Title,
            RepositoryFullName = link.Project?.GitHubOwner != null && link.Project.GitHubRepoName != null
                ? $"{link.Project.GitHubOwner}/{link.Project.GitHubRepoName}"
                : string.Empty,
            GitHubIssueNumber = link.GitHubIssueNumber,
            GitHubIssueUrl = link.GitHubIssueUrl,
            Status = link.Status.ToString(),
            Priority = link.Priority?.ToString(),
            CreatedAt = link.FechaCreacion
        };
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
