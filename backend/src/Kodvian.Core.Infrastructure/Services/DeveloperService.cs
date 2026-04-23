using Kodvian.Core.Application.Developers.Abstractions;
using Kodvian.Core.Application.Developers.Dtos;
using Kodvian.Core.Application.Developers.Requests;
using Kodvian.Core.Domain.Entities;
using Kodvian.Core.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kodvian.Core.Infrastructure.Services;

public class DeveloperService : IDeveloperService
{
    private readonly KodvianDbContext _dbContext;

    public DeveloperService(KodvianDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<DeveloperDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Developers
            .AsNoTracking()
            .OrderBy(x => x.FullName)
            .Select(x => new DeveloperDto
            {
                Id = x.Id,
                FullName = x.FullName,
                Email = x.Email,
                Phone = x.Phone,
                TaxId = x.TaxId,
                Notes = x.Notes,
                IsActive = x.Activo
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<DeveloperDto> CreateAsync(DeveloperUpsertRequestDto request, CancellationToken cancellationToken = default)
    {
        var developer = new Developer();
        ApplyRequest(developer, request);
        _dbContext.Developers.Add(developer);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return ToDto(developer);
    }

    public async Task<DeveloperDto?> UpdateAsync(Guid id, DeveloperUpsertRequestDto request, CancellationToken cancellationToken = default)
    {
        var developer = await _dbContext.Developers.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (developer is null)
        {
            return null;
        }

        ApplyRequest(developer, request);
        developer.FechaActualizacion = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToDto(developer);
    }

    private static void ApplyRequest(Developer target, DeveloperUpsertRequestDto request)
    {
        target.FullName = request.FullName.Trim();
        target.Email = Normalize(request.Email);
        target.Phone = Normalize(request.Phone);
        target.TaxId = Normalize(request.TaxId);
        target.Notes = Normalize(request.Notes);
        target.Activo = request.IsActive;
    }

    private static string? Normalize(string? value)
    {
        var normalized = value?.Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    private static DeveloperDto ToDto(Developer x)
    {
        return new DeveloperDto
        {
            Id = x.Id,
            FullName = x.FullName,
            Email = x.Email,
            Phone = x.Phone,
            TaxId = x.TaxId,
            Notes = x.Notes,
            IsActive = x.Activo
        };
    }
}
