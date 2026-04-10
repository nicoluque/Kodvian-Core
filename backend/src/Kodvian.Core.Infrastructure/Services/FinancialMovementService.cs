using System.Linq.Expressions;
using Kodvian.Core.Application.Common.Models;
using Kodvian.Core.Application.Finances.Abstractions;
using Kodvian.Core.Application.Finances.Dtos;
using Kodvian.Core.Application.Finances.Requests;
using Kodvian.Core.Domain.Entities;
using Kodvian.Core.Domain.Enums;
using Kodvian.Core.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kodvian.Core.Infrastructure.Services;

public class FinancialMovementService : IFinancialMovementService
{
    private readonly KodvianDbContext _dbContext;

    public FinancialMovementService(KodvianDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResultDto<FinancialMovementListItemDto>> GetPagedAsync(FinancialMovementListRequestDto request, CancellationToken cancellationToken = default)
    {
        var query = BuildFilteredQuery(request);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.MovementDate)
            .ThenByDescending(x => x.FechaCreacion)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new FinancialMovementListItemDto
            {
                Id = x.Id,
                MovementType = x.MovementType.ToString(),
                CategoryName = x.Category != null ? x.Category.Name : string.Empty,
                Description = x.Description,
                Amount = x.Amount,
                MovementDate = x.MovementDate,
                DueDate = x.DueDate,
                Status = x.Status.ToString(),
                PaymentMethod = x.PaymentMethod,
                ClientName = x.Client != null ? x.Client.CommercialName : null,
                ProviderName = x.Provider != null ? x.Provider.Name : null
            })
            .ToListAsync(cancellationToken);

        return new PagedResultDto<FinancialMovementListItemDto>
        {
            Items = items,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<FinancialMovementDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.FinancialMovements
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(ToDetailDto())
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<FinancialMovementDetailDto> CreateAsync(Guid createdById, FinancialMovementUpsertRequestDto request, CancellationToken cancellationToken = default)
    {
        await ValidateReferencesAsync(createdById, request, cancellationToken);

        var movement = new FinancialMovement
        {
            CreatedById = createdById
        };

        ApplyRequest(movement, request);

        _dbContext.FinancialMovements.Add(movement);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await _dbContext.FinancialMovements
            .AsNoTracking()
            .Where(x => x.Id == movement.Id)
            .Select(ToDetailDto())
            .FirstAsync(cancellationToken);
    }

    public async Task<FinancialMovementDetailDto?> UpdateAsync(Guid id, FinancialMovementUpsertRequestDto request, CancellationToken cancellationToken = default)
    {
        await ValidateReferencesAsync(null, request, cancellationToken);

        var movement = await _dbContext.FinancialMovements.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (movement is null)
        {
            return null;
        }

        ApplyRequest(movement, request);
        movement.FechaActualizacion = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await _dbContext.FinancialMovements
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(ToDetailDto())
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<FinanceMonthlySummaryDto> GetMonthlySummaryAsync(int? year, int? month, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var y = year ?? now.Year;
        var m = month ?? now.Month;
        var monthStart = new DateOnly(y, m, 1);
        var monthEnd = monthStart.AddMonths(1).AddDays(-1);

        var monthlyTotals = await _dbContext.FinancialMovements
            .AsNoTracking()
            .Where(x => x.MovementDate >= monthStart && x.MovementDate <= monthEnd && x.Status != FinancialMovementStatus.Anulado)
            .GroupBy(x => x.MovementType)
            .Select(g => new { MovementType = g.Key, Total = g.Sum(x => x.Amount) })
            .ToListAsync(cancellationToken);

        var pendingTotals = await _dbContext.FinancialMovements
            .AsNoTracking()
            .Where(x => x.Status == FinancialMovementStatus.Pendiente)
            .GroupBy(x => x.MovementType)
            .Select(g => new { MovementType = g.Key, Total = g.Sum(x => x.Amount) })
            .ToListAsync(cancellationToken);

        var income = monthlyTotals.Where(x => x.MovementType == FinancialMovementType.Ingreso).Select(x => x.Total).FirstOrDefault();
        var expense = monthlyTotals.Where(x => x.MovementType == FinancialMovementType.Egreso).Select(x => x.Total).FirstOrDefault();
        var pendingIncome = pendingTotals.Where(x => x.MovementType == FinancialMovementType.Ingreso).Select(x => x.Total).FirstOrDefault();
        var pendingExpense = pendingTotals.Where(x => x.MovementType == FinancialMovementType.Egreso).Select(x => x.Total).FirstOrDefault();

        return new FinanceMonthlySummaryDto
        {
            MonthlyIncome = income,
            MonthlyExpense = expense,
            MonthlyResult = income - expense,
            PendingIncome = pendingIncome,
            PendingExpense = pendingExpense
        };
    }

    public async Task<FinanceLookupsDto> GetLookupsAsync(CancellationToken cancellationToken = default)
    {
        var categories = await _dbContext.FinancialCategories
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Take(300)
            .Select(x => new FinancialCategoryDto
            {
                Id = x.Id,
                Name = x.Name,
                MovementType = x.MovementType.ToString(),
                IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);

        var clients = await _dbContext.Clients
            .AsNoTracking()
            .Where(x => x.Activo)
            .OrderBy(x => x.CommercialName)
            .Take(300)
            .Select(x => new FinanceLookupItemDto { Id = x.Id, Name = x.CommercialName })
            .ToListAsync(cancellationToken);

        var projects = await _dbContext.Projects
            .AsNoTracking()
            .Where(x => x.Activo)
            .OrderBy(x => x.Nombre)
            .Take(300)
            .Select(x => new FinanceLookupItemDto { Id = x.Id, Name = x.Nombre })
            .ToListAsync(cancellationToken);

        var providers = await _dbContext.Providers
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Take(300)
            .Select(x => new FinanceLookupItemDto { Id = x.Id, Name = x.Name })
            .ToListAsync(cancellationToken);

        return new FinanceLookupsDto
        {
            Categories = categories,
            Clients = clients,
            Projects = projects,
            Providers = providers
        };
    }

    private IQueryable<FinancialMovement> BuildFilteredQuery(FinancialMovementListRequestDto request)
    {
        var query = _dbContext.FinancialMovements
            .AsNoTracking()
            .AsQueryable();

        if (request.DateFrom.HasValue)
        {
            query = query.Where(x => x.MovementDate >= request.DateFrom.Value);
        }

        if (request.DateTo.HasValue)
        {
            query = query.Where(x => x.MovementDate <= request.DateTo.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.MovementType) && Enum.TryParse<FinancialMovementType>(request.MovementType, true, out var type))
        {
            query = query.Where(x => x.MovementType == type);
        }

        if (request.CategoryId.HasValue)
        {
            query = query.Where(x => x.CategoryId == request.CategoryId);
        }

        if (request.ClientId.HasValue)
        {
            query = query.Where(x => x.ClientId == request.ClientId);
        }

        if (request.ProviderId.HasValue)
        {
            query = query.Where(x => x.ProviderId == request.ProviderId);
        }

        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<FinancialMovementStatus>(request.Status, true, out var status))
        {
            query = query.Where(x => x.Status == status);
        }

        return query;
    }

    private static void ApplyRequest(FinancialMovement movement, FinancialMovementUpsertRequestDto request)
    {
        movement.MovementType = ParseMovementType(request.MovementType);
        movement.CategoryId = request.CategoryId;
        movement.ClientId = request.ClientId;
        movement.ProviderId = request.ProviderId;
        movement.ProjectId = request.ProjectId;
        movement.Description = request.Description.Trim();
        movement.Amount = request.Amount;
        movement.MovementDate = request.MovementDate;
        movement.DueDate = request.DueDate;
        movement.Status = ParseStatus(request.Status);
        movement.PaymentMethod = Normalize(request.PaymentMethod);
        movement.ReceiptNumber = Normalize(request.ReceiptNumber);
        movement.Notes = Normalize(request.Notes);
    }

    private static string? Normalize(string? value)
    {
        var normalized = value?.Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    private static FinancialMovementType ParseMovementType(string type)
    {
        return Enum.TryParse<FinancialMovementType>(type, true, out var parsed)
            ? parsed
            : FinancialMovementType.Ingreso;
    }

    private static FinancialMovementStatus ParseStatus(string status)
    {
        return Enum.TryParse<FinancialMovementStatus>(status, true, out var parsed)
            ? parsed
            : FinancialMovementStatus.Pendiente;
    }

    private static Expression<Func<FinancialMovement, FinancialMovementDetailDto>> ToDetailDto()
    {
        return x => new FinancialMovementDetailDto
        {
            Id = x.Id,
            MovementType = x.MovementType.ToString(),
            CategoryId = x.CategoryId,
            CategoryName = x.Category != null ? x.Category.Name : string.Empty,
            ClientId = x.ClientId,
            ClientName = x.Client != null ? x.Client.CommercialName : null,
            ProviderId = x.ProviderId,
            ProviderName = x.Provider != null ? x.Provider.Name : null,
            ProjectId = x.ProjectId,
            ProjectName = x.Project != null ? x.Project.Nombre : null,
            Description = x.Description,
            Amount = x.Amount,
            MovementDate = x.MovementDate,
            DueDate = x.DueDate,
            Status = x.Status.ToString(),
            PaymentMethod = x.PaymentMethod,
            ReceiptNumber = x.ReceiptNumber,
            Notes = x.Notes,
            CreatedById = x.CreatedById,
            CreatedByName = x.CreatedBy != null ? x.CreatedBy.FullName : string.Empty,
            CreatedAt = x.FechaCreacion,
            UpdatedAt = x.FechaActualizacion
        };
    }

    private async Task ValidateReferencesAsync(Guid? createdById, FinancialMovementUpsertRequestDto request, CancellationToken cancellationToken)
    {
        var categoryExists = await _dbContext.FinancialCategories.AnyAsync(x => x.Id == request.CategoryId, cancellationToken);
        if (!categoryExists)
        {
            throw new ArgumentException("La categoria seleccionada no existe");
        }

        if (request.ClientId.HasValue)
        {
            var clientExists = await _dbContext.Clients.AnyAsync(x => x.Id == request.ClientId.Value, cancellationToken);
            if (!clientExists)
            {
                throw new ArgumentException("El cliente seleccionado no existe");
            }
        }

        if (request.ProviderId.HasValue)
        {
            var providerExists = await _dbContext.Providers.AnyAsync(x => x.Id == request.ProviderId.Value, cancellationToken);
            if (!providerExists)
            {
                throw new ArgumentException("El proveedor seleccionado no existe");
            }
        }

        if (request.ProjectId.HasValue)
        {
            var projectExists = await _dbContext.Projects.AnyAsync(x => x.Id == request.ProjectId.Value, cancellationToken);
            if (!projectExists)
            {
                throw new ArgumentException("El proyecto seleccionado no existe");
            }
        }

        if (createdById.HasValue)
        {
            var creatorExists = await _dbContext.Users.AnyAsync(x => x.Id == createdById.Value, cancellationToken);
            if (!creatorExists)
            {
                throw new ArgumentException("El usuario creador no existe");
            }
        }
    }
}
