using Kodvian.Core.Application.Finances.Abstractions;
using Kodvian.Core.Application.Finances.Dtos;
using Kodvian.Core.Application.Finances.Requests;
using Kodvian.Core.Domain.Entities;
using Kodvian.Core.Domain.Enums;
using Kodvian.Core.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kodvian.Core.Infrastructure.Services;

public class FinancialCategoryService : IFinancialCategoryService
{
    private readonly KodvianDbContext _dbContext;

    public FinancialCategoryService(KodvianDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<FinancialCategoryDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.FinancialCategories
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new FinancialCategoryDto
            {
                Id = x.Id,
                Name = x.Name,
                MovementType = x.MovementType.ToString(),
                IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<FinancialCategoryDto> CreateAsync(FinancialCategoryUpsertRequestDto request, CancellationToken cancellationToken = default)
    {
        var category = new FinancialCategory
        {
            Name = request.Name.Trim(),
            MovementType = ParseMovementType(request.MovementType),
            IsActive = request.IsActive
        };

        _dbContext.FinancialCategories.Add(category);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new FinancialCategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            MovementType = category.MovementType.ToString(),
            IsActive = category.IsActive
        };
    }

    public async Task<FinancialCategoryDto?> UpdateAsync(Guid id, FinancialCategoryUpsertRequestDto request, CancellationToken cancellationToken = default)
    {
        var category = await _dbContext.FinancialCategories.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (category is null)
        {
            return null;
        }

        category.Name = request.Name.Trim();
        category.MovementType = ParseMovementType(request.MovementType);
        category.IsActive = request.IsActive;
        category.FechaActualizacion = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new FinancialCategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            MovementType = category.MovementType.ToString(),
            IsActive = category.IsActive
        };
    }

    private static FinancialMovementType ParseMovementType(string type)
    {
        return Enum.TryParse<FinancialMovementType>(type, true, out var parsed)
            ? parsed
            : FinancialMovementType.Ingreso;
    }
}
