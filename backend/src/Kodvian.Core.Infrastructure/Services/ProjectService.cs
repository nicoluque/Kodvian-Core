using System.Linq.Expressions;
using Kodvian.Core.Application.Common.Models;
using Kodvian.Core.Application.Projects.Abstractions;
using Kodvian.Core.Application.Projects.Dtos;
using Kodvian.Core.Application.Projects.Requests;
using Kodvian.Core.Domain.Entities;
using Kodvian.Core.Domain.Enums;
using Kodvian.Core.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kodvian.Core.Infrastructure.Services;

public class ProjectService : IProjectService
{
    private readonly KodvianDbContext _dbContext;

    public ProjectService(KodvianDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResultDto<ProjectListItemDto>> GetPagedAsync(ProjectListRequestDto request, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Projects
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = $"%{request.Search.Trim()}%";
            query = query.Where(x => EF.Functions.ILike(x.Nombre, search));
        }

        if (request.ClientId.HasValue)
        {
            query = query.Where(x => x.ClienteId == request.ClientId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<ProjectStatus>(request.Status, true, out var status))
        {
            query = query.Where(x => x.Estado == status);
        }

        if (!string.IsNullOrWhiteSpace(request.Priority) && Enum.TryParse<ProjectPriority>(request.Priority, true, out var priority))
        {
            query = query.Where(x => x.Prioridad == priority);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.FechaCreacion)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new ProjectListItemDto
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
            })
            .ToListAsync(cancellationToken);

        return new PagedResultDto<ProjectListItemDto>
        {
            Items = items,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<ProjectDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Projects
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(ToDetailDto())
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ProjectDetailDto> CreateAsync(ProjectUpsertRequestDto request, CancellationToken cancellationToken = default)
    {
        await ValidateReferencesAsync(request, cancellationToken);

        var project = new Project();
        ApplyRequest(project, request);

        _dbContext.Projects.Add(project);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await _dbContext.Projects
            .AsNoTracking()
            .Where(x => x.Id == project.Id)
            .Select(ToDetailDto())
            .FirstAsync(cancellationToken);
    }

    public async Task<ProjectDetailDto?> UpdateAsync(Guid id, ProjectUpsertRequestDto request, CancellationToken cancellationToken = default)
    {
        await ValidateReferencesAsync(request, cancellationToken);

        var project = await _dbContext.Projects.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (project is null)
        {
            return null;
        }

        ApplyRequest(project, request);
        project.FechaActualizacion = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await _dbContext.Projects
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(ToDetailDto())
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ProjectLookupsDto> GetLookupsAsync(CancellationToken cancellationToken = default)
    {
        var clients = await _dbContext.Clients
            .AsNoTracking()
            .Where(x => x.Activo)
            .OrderBy(x => x.CommercialName)
            .Take(300)
            .Select(x => new ProjectLookupItemDto
            {
                Id = x.Id,
                Name = x.CommercialName
            })
            .ToListAsync(cancellationToken);

        var responsibles = await _dbContext.Users
            .AsNoTracking()
            .Where(x => x.Activo)
            .OrderBy(x => x.FullName)
            .Take(300)
            .Select(x => new ProjectLookupItemDto
            {
                Id = x.Id,
                Name = x.FullName
            })
            .ToListAsync(cancellationToken);

        return new ProjectLookupsDto
        {
            Clients = clients,
            Responsibles = responsibles
        };
    }

    private static void ApplyRequest(Project project, ProjectUpsertRequestDto request)
    {
        project.ClienteId = request.ClientId;
        project.Nombre = request.Name.Trim();
        project.Descripcion = Normalize(request.Description);
        project.ResponsableId = request.ResponsibleId;
        project.Estado = ParseStatus(request.Status);
        project.Prioridad = ParsePriority(request.Priority);
        project.FechaInicio = request.StartDate;
        project.FechaEntregaEstimada = request.EstimatedDeliveryDate;
        project.FechaCierre = request.ClosingDate;
        project.Presupuesto = request.Budget;
        project.PorcentajeAvance = request.ProgressPercentage;
        project.Activo = request.IsActive;
    }

    private static string? Normalize(string? value)
    {
        var normalized = value?.Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    private static ProjectStatus ParseStatus(string status)
    {
        return Enum.TryParse<ProjectStatus>(status, true, out var parsed)
            ? parsed
            : ProjectStatus.Planificacion;
    }

    private static ProjectPriority ParsePriority(string priority)
    {
        return Enum.TryParse<ProjectPriority>(priority, true, out var parsed)
            ? parsed
            : ProjectPriority.Media;
    }

    private static Expression<Func<Project, ProjectDetailDto>> ToDetailDto()
    {
        return x => new ProjectDetailDto
        {
            Id = x.Id,
            ClientId = x.ClienteId,
            ClientName = x.Cliente != null ? x.Cliente.CommercialName : string.Empty,
            Name = x.Nombre,
            Description = x.Descripcion,
            ResponsibleId = x.ResponsableId,
            ResponsibleName = x.Responsable != null ? x.Responsable.FullName : null,
            Status = x.Estado.ToString(),
            Priority = x.Prioridad.ToString(),
            StartDate = x.FechaInicio,
            EstimatedDeliveryDate = x.FechaEntregaEstimada,
            ClosingDate = x.FechaCierre,
            Budget = x.Presupuesto,
            ProgressPercentage = x.PorcentajeAvance,
            IsActive = x.Activo,
            CreatedAt = x.FechaCreacion,
            UpdatedAt = x.FechaActualizacion
        };
    }

    private async Task ValidateReferencesAsync(ProjectUpsertRequestDto request, CancellationToken cancellationToken)
    {
        var clientExists = await _dbContext.Clients.AnyAsync(x => x.Id == request.ClientId, cancellationToken);
        if (!clientExists)
        {
            throw new ArgumentException("El cliente seleccionado no existe");
        }

        if (request.ResponsibleId.HasValue)
        {
            var userExists = await _dbContext.Users.AnyAsync(x => x.Id == request.ResponsibleId.Value, cancellationToken);
            if (!userExists)
            {
                throw new ArgumentException("El responsable seleccionado no existe");
            }
        }
    }
}
