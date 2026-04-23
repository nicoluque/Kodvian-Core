namespace Kodvian.Core.Application.Developers.Dtos;

public class ContractLedgerMonthDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal ProjectIncomeBase { get; set; }
    public decimal DueAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal Balance { get; set; }
}
