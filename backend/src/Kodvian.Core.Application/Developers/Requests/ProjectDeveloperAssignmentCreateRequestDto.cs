using System.ComponentModel.DataAnnotations;

namespace Kodvian.Core.Application.Developers.Requests;

public class ProjectDeveloperAssignmentCreateRequestDto
{
    [Required]
    public Guid DeveloperId { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }
}
