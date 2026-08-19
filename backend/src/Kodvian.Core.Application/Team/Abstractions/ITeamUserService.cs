using Kodvian.Core.Application.Team.Dtos;
using Kodvian.Core.Application.Team.Requests;

namespace Kodvian.Core.Application.Team.Abstractions;

public interface ITeamUserService
{
    Task<IReadOnlyCollection<TeamUserDto>> GetAnalystsAsync(CancellationToken cancellationToken = default);
    Task<TeamUserDto> CreateAnalystAsync(TeamUserUpsertRequestDto request, CancellationToken cancellationToken = default);
    Task<TeamUserDto?> UpdateAnalystAsync(Guid id, TeamUserUpsertRequestDto request, CancellationToken cancellationToken = default);
}
