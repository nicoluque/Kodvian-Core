namespace Kodvian.Core.Application.Finances.Requests;

public class ProviderUpsertRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string? TaxId { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public bool IsActive { get; set; } = true;
}
