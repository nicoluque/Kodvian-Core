using Kodvian.Core.Application.Dashboard.Dtos;
using Kodvian.Core.Application.Dashboard.Requests;

namespace Kodvian.Core.Application.Dashboard.Abstractions;

public interface IDashboardService
{
    Task<DashboardOverviewDto> GetOverviewAsync(DashboardOverviewRequestDto request, CancellationToken cancellationToken = default);
}
