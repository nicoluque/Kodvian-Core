using System.Linq.Expressions;
using Kodvian.Core.Application.Developers.Abstractions;
using Kodvian.Core.Application.Developers.Dtos;
using Kodvian.Core.Application.Developers.Requests;
using Kodvian.Core.Domain.Entities;
using Kodvian.Core.Domain.Enums;
using Kodvian.Core.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kodvian.Core.Infrastructure.Services;

public class ProjectDeveloperContractService : IProjectDeveloperContractService
{
    private readonly KodvianDbContext _dbContext;

    public ProjectDeveloperContractService(KodvianDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<ProjectDeveloperContractDto>> GetByProjectAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ProjectDeveloperContracts
            .AsNoTracking()
            .Where(x => x.ProjectId == projectId)
            .OrderByDescending(x => x.Activo)
            .ThenByDescending(x => x.FechaCreacion)
            .Select(ToDto())
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<DeveloperContractSummaryDto>> GetByDeveloperSummaryAsync(Guid developerId, int year, CancellationToken cancellationToken = default)
    {
        var contracts = await _dbContext.ProjectDeveloperContracts
            .AsNoTracking()
            .Where(x => x.DeveloperId == developerId)
            .OrderByDescending(x => x.Activo)
            .ThenByDescending(x => x.FechaCreacion)
            .Select(x => new
            {
                x.Id,
                x.ProjectId,
                ProjectName = x.Project != null ? x.Project.Nombre : string.Empty,
                x.PaymentMode,
                x.Percentage,
                x.AgreedAmount,
                x.Activo
            })
            .ToListAsync(cancellationToken);

        if (contracts.Count == 0)
        {
            return Array.Empty<DeveloperContractSummaryDto>();
        }

        var contractIds = contracts.Select(x => x.Id).ToList();

        var lastPaymentByContract = await _dbContext.DeveloperPayments
            .AsNoTracking()
            .Where(x => contractIds.Contains(x.ContractId))
            .GroupBy(x => x.ContractId)
            .Select(g => new
            {
                ContractId = g.Key,
                LastPaymentDate = g.Max(x => x.PaymentDate)
            })
            .ToDictionaryAsync(x => x.ContractId, x => (DateOnly?)x.LastPaymentDate, cancellationToken);

        var result = new List<DeveloperContractSummaryDto>(contracts.Count);

        foreach (var contract in contracts)
        {
            var ledger = await GetLedgerAsync(contract.Id, year, cancellationToken);
            if (ledger is null)
            {
                continue;
            }

            lastPaymentByContract.TryGetValue(contract.Id, out var lastPaymentDate);

            result.Add(new DeveloperContractSummaryDto
            {
                ContractId = contract.Id,
                ProjectId = contract.ProjectId,
                ProjectName = contract.ProjectName,
                PaymentMode = contract.PaymentMode.ToString(),
                Percentage = contract.Percentage,
                AgreedAmount = contract.AgreedAmount,
                TotalDue = ledger.TotalDue,
                TotalPaid = ledger.TotalPaid,
                TotalBalance = ledger.TotalBalance,
                IsUpToDate = ledger.TotalBalance <= 0,
                LastPaymentDate = lastPaymentDate,
                IsActive = contract.Activo
            });
        }

        return result;
    }

    public async Task<ProjectDeveloperContractDto> CreateAsync(Guid projectId, ProjectDeveloperContractUpsertRequestDto request, CancellationToken cancellationToken = default)
    {
        await ValidateReferencesAndConflictsAsync(projectId, request, null, cancellationToken);

        var entity = new ProjectDeveloperContract
        {
            ProjectId = projectId
        };

        ApplyRequest(entity, request);
        _dbContext.ProjectDeveloperContracts.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await _dbContext.ProjectDeveloperContracts
            .AsNoTracking()
            .Where(x => x.Id == entity.Id)
            .Select(ToDto())
            .FirstAsync(cancellationToken);
    }

    public async Task<ProjectDeveloperContractDto?> UpdateAsync(Guid id, ProjectDeveloperContractUpsertRequestDto request, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.ProjectDeveloperContracts.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        await ValidateReferencesAndConflictsAsync(entity.ProjectId, request, id, cancellationToken);

        ApplyRequest(entity, request);
        entity.FechaActualizacion = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await _dbContext.ProjectDeveloperContracts
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(ToDto())
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ContractLedgerDto?> GetLedgerAsync(Guid contractId, int year, CancellationToken cancellationToken = default)
    {
        var contract = await _dbContext.ProjectDeveloperContracts
            .AsNoTracking()
            .Where(x => x.Id == contractId)
            .Select(x => new
            {
                x.Id,
                x.ProjectId,
                ProjectName = x.Project != null ? x.Project.Nombre : string.Empty,
                x.DeveloperId,
                DeveloperName = x.Developer != null ? x.Developer.FullName : string.Empty,
                x.PaymentMode,
                x.Percentage,
                x.AgreedAmount,
                x.StartDate,
                x.EndDate
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (contract is null)
        {
            return null;
        }

        var periodStart = new DateOnly(year, 1, 1);
        var periodEnd = new DateOnly(year, 12, 31);
        var activeStart = contract.StartDate > periodStart ? contract.StartDate : periodStart;
        var activeEnd = contract.EndDate.HasValue && contract.EndDate.Value < periodEnd ? contract.EndDate.Value : periodEnd;

        var incomeByMonth = await _dbContext.FinancialMovements
            .AsNoTracking()
            .Where(x => x.ProjectId == contract.ProjectId
                        && x.MovementType == FinancialMovementType.Ingreso
                        && (x.Status == FinancialMovementStatus.Pendiente || x.Status == FinancialMovementStatus.Cobrado)
                        && x.MovementDate >= activeStart
                        && x.MovementDate <= activeEnd)
            .GroupBy(x => x.MovementDate.Month)
            .Select(g => new { Month = g.Key, Amount = g.Sum(x => x.Amount) })
            .ToDictionaryAsync(x => x.Month, x => x.Amount, cancellationToken);

        var paidByMonth = await _dbContext.DeveloperPayments
            .AsNoTracking()
            .Where(x => x.ContractId == contractId && x.PeriodYear == year)
            .GroupBy(x => x.PeriodMonth)
            .Select(g => new { Month = g.Key, Amount = g.Sum(x => x.Amount) })
            .ToDictionaryAsync(x => x.Month, x => x.Amount, cancellationToken);

        var months = new List<ContractLedgerMonthDto>();
        var totalDue = 0m;
        var totalPaid = 0m;

        for (var month = 1; month <= 12; month++)
        {
            var monthStartDate = new DateOnly(year, month, 1);
            var monthEndDate = monthStartDate.AddMonths(1).AddDays(-1);
            if (monthEndDate < activeStart || monthStartDate > activeEnd)
            {
                continue;
            }

            incomeByMonth.TryGetValue(month, out var incomeBase);
            paidByMonth.TryGetValue(month, out var paidAmount);

            var dueAmount = contract.PaymentMode == ContractPaymentMode.Percentage
                ? Math.Round(incomeBase * ((contract.Percentage ?? 0m) / 100m), 2, MidpointRounding.AwayFromZero)
                : 0m;

            months.Add(new ContractLedgerMonthDto
            {
                Year = year,
                Month = month,
                ProjectIncomeBase = incomeBase,
                DueAmount = dueAmount,
                PaidAmount = paidAmount,
                Balance = dueAmount - paidAmount
            });

            totalDue += dueAmount;
            totalPaid += paidAmount;
        }

        if (contract.PaymentMode == ContractPaymentMode.FixedAmount)
        {
            var fixedTotal = contract.AgreedAmount ?? 0m;
            totalDue = fixedTotal;
            months = new List<ContractLedgerMonthDto>
            {
                new ContractLedgerMonthDto
                {
                    Year = year,
                    Month = 1,
                    ProjectIncomeBase = 0m,
                    DueAmount = fixedTotal,
                    PaidAmount = totalPaid,
                    Balance = fixedTotal - totalPaid
                }
            };
        }

        return new ContractLedgerDto
        {
            ContractId = contract.Id,
            ProjectId = contract.ProjectId,
            ProjectName = contract.ProjectName,
            DeveloperId = contract.DeveloperId,
            DeveloperName = contract.DeveloperName,
            PaymentMode = contract.PaymentMode.ToString(),
            Percentage = contract.Percentage,
            AgreedAmount = contract.AgreedAmount,
            TotalDue = totalDue,
            TotalPaid = totalPaid,
            TotalBalance = totalDue - totalPaid,
            Months = months
        };
    }

    private static void ApplyRequest(ProjectDeveloperContract entity, ProjectDeveloperContractUpsertRequestDto request)
    {
        entity.DeveloperId = request.DeveloperId;
        entity.PaymentMode = ParsePaymentMode(request.PaymentMode);
        entity.Percentage = request.Percentage;
        entity.AgreedAmount = request.AgreedAmount;
        entity.StartDate = request.StartDate;
        entity.EndDate = request.EndDate;
        entity.Activo = request.IsActive;
        entity.Notes = Normalize(request.Notes);
    }

    private static string? Normalize(string? value)
    {
        var normalized = value?.Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    private static ContractPaymentMode ParsePaymentMode(string mode)
    {
        return Enum.TryParse<ContractPaymentMode>(mode, true, out var parsed)
            ? parsed
            : ContractPaymentMode.Percentage;
    }

    private static Expression<Func<ProjectDeveloperContract, ProjectDeveloperContractDto>> ToDto()
    {
        return x => new ProjectDeveloperContractDto
        {
            Id = x.Id,
            ProjectId = x.ProjectId,
            ProjectName = x.Project != null ? x.Project.Nombre : string.Empty,
            DeveloperId = x.DeveloperId,
            DeveloperName = x.Developer != null ? x.Developer.FullName : string.Empty,
            PaymentMode = x.PaymentMode.ToString(),
            Percentage = x.Percentage,
            AgreedAmount = x.AgreedAmount,
            StartDate = x.StartDate,
            EndDate = x.EndDate,
            IsActive = x.Activo,
            Notes = x.Notes
        };
    }

    private async Task ValidateReferencesAndConflictsAsync(Guid projectId, ProjectDeveloperContractUpsertRequestDto request, Guid? currentId, CancellationToken cancellationToken)
    {
        var projectExists = await _dbContext.Projects.AnyAsync(x => x.Id == projectId, cancellationToken);
        if (!projectExists)
        {
            throw new ArgumentException("El proyecto seleccionado no existe");
        }

        var developerExists = await _dbContext.Developers.AnyAsync(x => x.Id == request.DeveloperId, cancellationToken);
        if (!developerExists)
        {
            throw new ArgumentException("El desarrollador seleccionado no existe");
        }

        var parsedMode = ParsePaymentMode(request.PaymentMode);
        if (parsedMode == ContractPaymentMode.Percentage)
        {
            if (!request.Percentage.HasValue || request.Percentage <= 0 || request.Percentage > 100)
            {
                throw new ArgumentException("El porcentaje debe ser mayor a 0 y menor o igual a 100");
            }
        }

        if (parsedMode == ContractPaymentMode.FixedAmount)
        {
            if (!request.AgreedAmount.HasValue || request.AgreedAmount <= 0)
            {
                throw new ArgumentException("El monto acordado debe ser mayor a 0");
            }
        }

        if (request.EndDate.HasValue && request.EndDate < request.StartDate)
        {
            throw new ArgumentException("La fecha de finalización no puede ser anterior a la fecha de inicio");
        }

        if (!request.IsActive)
        {
            return;
        }

        var conflictExists = await _dbContext.ProjectDeveloperContracts
            .AnyAsync(x => x.ProjectId == projectId
                           && x.DeveloperId == request.DeveloperId
                           && x.Activo
                           && (!currentId.HasValue || x.Id != currentId.Value), cancellationToken);

        if (conflictExists)
        {
            throw new ArgumentException("Ya existe un contrato activo para este desarrollador en el proyecto");
        }
    }
}
