namespace GymFlow.Application.Modules.Dashboard;

public class DashboardStatsDto
{
    public int TotalMembers { get; set; }
    public int ActiveMembers { get; set; }
    public int NewMembersThisMonth { get; set; }
    public int ExpiringSoon { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public decimal RevenueGrowth { get; set; }
    public decimal PendingPayments { get; set; }
    public int TotalProducts { get; set; }
    public int LowStockItems { get; set; }
    public int TotalStaff { get; set; }
    public int TodayCheckIns { get; set; }
    public int OccupancyRate { get; set; }
}

public record RevenueChartItem(string Month, decimal Revenue, decimal Expenses);
public record RevenueByTypeItem(string Type, decimal Amount);
public record RecentActivityDto(string Id, string Type, string Message, string Time, string Icon);

public interface IDashboardService
{
    Task<DashboardStatsDto> GetStatsAsync(Guid gymId);
    Task<List<RecentActivityDto>> GetRecentActivityAsync(Guid gymId);
    Task<List<RevenueChartItem>> GetRevenueChartAsync(Guid gymId);
    Task<object> GetSummaryAsync(Guid gymId);
}
