namespace Kodvian.Core.Application.Projects.Dtos;

public class ProjectLookupsDto
{
    public IReadOnlyCollection<ProjectLookupItemDto> Clients { get; set; } = Array.Empty<ProjectLookupItemDto>();
    public IReadOnlyCollection<ProjectLookupItemDto> Responsibles { get; set; } = Array.Empty<ProjectLookupItemDto>();
}
