namespace Kodvian.Core.Application.Dashboard.Dtos;

public class DashboardKpisDto
{
    public int ActiveClients { get; set; }
    public int ProjectsInProgress { get; set; }
    public int OverdueTasks { get; set; }
    public int TasksForToday { get; set; }
    public decimal MonthlyIncome { get; set; }
    public decimal MonthlyExpense { get; set; }
    public decimal MonthlyResult { get; set; }
    public decimal PendingCollections { get; set; }
    public decimal PendingPayments { get; set; }
}
