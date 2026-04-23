namespace Kodvian.Core.Application.Developers.Requests;

public class DeveloperUpsertRequestDto
{
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? TaxId { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
}
