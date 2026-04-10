using Kodvian.Core.Application.Common.Models;

namespace Kodvian.Core.Application.Finances.Requests;

public class FinancialMovementListRequestDto : PagedRequestDto
{
    public DateOnly? DateFrom { get; set; }
    public DateOnly? DateTo { get; set; }
    public string? MovementType { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? ClientId { get; set; }
    public Guid? ProviderId { get; set; }
    public string? Status { get; set; }
}
