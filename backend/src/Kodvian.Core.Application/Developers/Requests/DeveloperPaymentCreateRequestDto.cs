namespace Kodvian.Core.Application.Developers.Requests;

public class DeveloperPaymentCreateRequestDto
{
    public DateOnly PaymentDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    public decimal Amount { get; set; }
    public int PeriodYear { get; set; }
    public int PeriodMonth { get; set; }
    public string? Reference { get; set; }
    public string? Notes { get; set; }
}
