using System.ComponentModel.DataAnnotations;

namespace Kodvian.Core.Application.Team.Requests;

public class TeamUserUpsertRequestDto
{
    [Required]
    [MaxLength(160)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [MaxLength(120)]
    public string Email { get; set; } = string.Empty;

    [MinLength(8)]
    public string? Password { get; set; }

    public bool IsActive { get; set; } = true;
}
