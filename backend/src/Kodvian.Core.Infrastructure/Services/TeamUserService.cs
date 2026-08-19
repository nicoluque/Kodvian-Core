using System.Linq.Expressions;
using Kodvian.Core.Application.Auth.Abstractions;
using Kodvian.Core.Application.Common.Security;
using Kodvian.Core.Application.Team.Abstractions;
using Kodvian.Core.Application.Team.Dtos;
using Kodvian.Core.Application.Team.Requests;
using Kodvian.Core.Domain.Entities;
using Kodvian.Core.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kodvian.Core.Infrastructure.Services;

public class TeamUserService : ITeamUserService
{
    private readonly KodvianDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;

    public TeamUserService(KodvianDbContext dbContext, IPasswordHasher passwordHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }

    public async Task<IReadOnlyCollection<TeamUserDto>> GetAnalystsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .Where(x => x.Role != null && x.Role.Name == RoleNames.Analyst)
            .OrderByDescending(x => x.Activo)
            .ThenBy(x => x.FullName)
            .Select(ToDto())
            .ToListAsync(cancellationToken);
    }

    public async Task<TeamUserDto> CreateAnalystAsync(TeamUserUpsertRequestDto request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new InvalidOperationException("La contraseña inicial es obligatoria");
        }

        var email = NormalizeEmail(request.Email);
        var exists = await _dbContext.Users.AnyAsync(x => x.Email == email, cancellationToken);
        if (exists)
        {
            throw new InvalidOperationException("Ya existe un usuario con ese email");
        }

        var analystRole = await _dbContext.Roles.FirstOrDefaultAsync(x => x.Name == RoleNames.Analyst, cancellationToken);
        if (analystRole is null)
        {
            throw new InvalidOperationException("El rol Analista no está configurado");
        }

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = email,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            RoleId = analystRole.Id,
            Activo = request.IsActive
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return await GetAnalystByIdAsync(user.Id, cancellationToken) ?? throw new InvalidOperationException("No se pudo recuperar el usuario creado");
    }

    public async Task<TeamUserDto?> UpdateAnalystAsync(Guid id, TeamUserUpsertRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == id && x.Role != null && x.Role.Name == RoleNames.Analyst, cancellationToken);

        if (user is null)
        {
            return null;
        }

        var email = NormalizeEmail(request.Email);
        var emailExists = await _dbContext.Users.AnyAsync(x => x.Id != id && x.Email == email, cancellationToken);
        if (emailExists)
        {
            throw new InvalidOperationException("Ya existe un usuario con ese email");
        }

        user.FullName = request.FullName.Trim();
        user.Email = email;
        user.Activo = request.IsActive;
        user.FechaActualizacion = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            user.PasswordHash = _passwordHasher.HashPassword(request.Password);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return await GetAnalystByIdAsync(id, cancellationToken);
    }

    private async Task<TeamUserDto?> GetAnalystByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .Where(x => x.Id == id && x.Role != null && x.Role.Name == RoleNames.Analyst)
            .Select(ToDto())
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static Expression<Func<User, TeamUserDto>> ToDto()
    {
        return x => new TeamUserDto
        {
            Id = x.Id,
            FullName = x.FullName,
            Email = x.Email,
            Role = x.Role != null ? x.Role.Name : string.Empty,
            IsActive = x.Activo,
            CreatedAt = x.FechaCreacion,
            UpdatedAt = x.FechaActualizacion
        };
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }
}
