using Kodvian.Core.Application.Developers.Abstractions;
using Kodvian.Core.Application.Developers.Dtos;
using Kodvian.Core.Application.Developers.Requests;
using Kodvian.Core.Application.Auth.Abstractions;
using Kodvian.Core.Application.Common.Security;
using Kodvian.Core.Domain.Entities;
using Kodvian.Core.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kodvian.Core.Infrastructure.Services;

public class DeveloperService : IDeveloperService
{
    private readonly KodvianDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;

    public DeveloperService(KodvianDbContext dbContext, IPasswordHasher passwordHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
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
                IsActive = x.Activo,
                HasSystemAccess = x.Users.Any(u => u.Role != null && u.Role.Name == RoleNames.Developer),
                IsSystemAccessActive = x.Users.Any(u => u.Role != null && u.Role.Name == RoleNames.Developer && u.Activo)
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<DeveloperDto> CreateAsync(DeveloperUpsertRequestDto request, CancellationToken cancellationToken = default)
    {
        var developer = new Developer();
        ApplyRequest(developer, request);
        _dbContext.Developers.Add(developer);
        await ApplySystemAccessAsync(developer, request, isCreate: true, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return await GetDtoAsync(developer.Id, cancellationToken);
    }

    public async Task<DeveloperDto?> UpdateAsync(Guid id, DeveloperUpsertRequestDto request, CancellationToken cancellationToken = default)
    {
        var developer = await _dbContext.Developers.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (developer is null)
        {
            return null;
        }

        ApplyRequest(developer, request);
        await ApplySystemAccessAsync(developer, request, isCreate: false, cancellationToken);
        developer.FechaActualizacion = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetDtoAsync(developer.Id, cancellationToken);
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

    private async Task ApplySystemAccessAsync(Developer developer, DeveloperUpsertRequestDto request, bool isCreate, CancellationToken cancellationToken)
    {
        var existingUser = await _dbContext.Users
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.DeveloperId == developer.Id && x.Role != null && x.Role.Name == RoleNames.Developer, cancellationToken);

        if (!request.EnableSystemAccess)
        {
            if (existingUser is not null)
            {
                existingUser.Activo = false;
                existingUser.FechaActualizacion = DateTime.UtcNow;
            }

            return;
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new InvalidOperationException("El email es obligatorio para habilitar el acceso al sistema.");
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var emailOwnerExists = await _dbContext.Users.AnyAsync(x => x.Email == normalizedEmail && x.DeveloperId != developer.Id, cancellationToken);
        if (emailOwnerExists)
        {
            throw new InvalidOperationException("Ya existe un usuario con ese email.");
        }

        var role = await _dbContext.Roles.FirstOrDefaultAsync(x => x.Name == RoleNames.Developer, cancellationToken)
            ?? throw new InvalidOperationException("El rol Desarrollador no está configurado.");

        if (existingUser is null)
        {
            if (string.IsNullOrWhiteSpace(request.AccessPassword))
            {
                throw new InvalidOperationException("La contraseña inicial es obligatoria para crear el acceso.");
            }

            _dbContext.Users.Add(new User
            {
                FullName = developer.FullName,
                Email = normalizedEmail,
                PasswordHash = _passwordHasher.HashPassword(request.AccessPassword),
                RoleId = role.Id,
                DeveloperId = developer.Id,
                Activo = request.IsSystemAccessActive
            });
            return;
        }

        existingUser.FullName = developer.FullName;
        existingUser.Email = normalizedEmail;
        existingUser.RoleId = role.Id;
        existingUser.Activo = request.IsSystemAccessActive;
        existingUser.FechaActualizacion = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(request.AccessPassword))
        {
            existingUser.PasswordHash = _passwordHasher.HashPassword(request.AccessPassword);
        }
    }

    private async Task<DeveloperDto> GetDtoAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.Developers
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new DeveloperDto
            {
                Id = x.Id,
                FullName = x.FullName,
                Email = x.Email,
                Phone = x.Phone,
                TaxId = x.TaxId,
                Notes = x.Notes,
                IsActive = x.Activo,
                HasSystemAccess = x.Users.Any(u => u.Role != null && u.Role.Name == RoleNames.Developer),
                IsSystemAccessActive = x.Users.Any(u => u.Role != null && u.Role.Name == RoleNames.Developer && u.Activo)
            })
            .FirstAsync(cancellationToken);
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
