using Kodvian.Core.Application.Finances.Abstractions;
using Kodvian.Core.Application.Finances.Dtos;
using Kodvian.Core.Application.Finances.Requests;
using Kodvian.Core.Domain.Entities;
using Kodvian.Core.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kodvian.Core.Infrastructure.Services;

public class ProviderService : IProviderService
{
    private readonly KodvianDbContext _dbContext;

    public ProviderService(KodvianDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<ProviderDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Providers
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new ProviderDto
            {
                Id = x.Id,
                Name = x.Name,
                TaxId = x.TaxId,
                Email = x.Email,
                Phone = x.Phone,
                IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<ProviderDto> CreateAsync(ProviderUpsertRequestDto request, CancellationToken cancellationToken = default)
    {
        var provider = new Provider
        {
            Name = request.Name.Trim(),
            TaxId = Normalize(request.TaxId),
            Email = Normalize(request.Email),
            Phone = Normalize(request.Phone),
            IsActive = request.IsActive
        };

        _dbContext.Providers.Add(provider);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ProviderDto
        {
            Id = provider.Id,
            Name = provider.Name,
            TaxId = provider.TaxId,
            Email = provider.Email,
            Phone = provider.Phone,
            IsActive = provider.IsActive
        };
    }

    public async Task<ProviderDto?> UpdateAsync(Guid id, ProviderUpsertRequestDto request, CancellationToken cancellationToken = default)
    {
        var provider = await _dbContext.Providers.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (provider is null)
        {
            return null;
        }

        provider.Name = request.Name.Trim();
        provider.TaxId = Normalize(request.TaxId);
        provider.Email = Normalize(request.Email);
        provider.Phone = Normalize(request.Phone);
        provider.IsActive = request.IsActive;
        provider.FechaActualizacion = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ProviderDto
        {
            Id = provider.Id,
            Name = provider.Name,
            TaxId = provider.TaxId,
            Email = provider.Email,
            Phone = provider.Phone,
            IsActive = provider.IsActive
        };
    }

    private static string? Normalize(string? value)
    {
        var normalized = value?.Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }
}
