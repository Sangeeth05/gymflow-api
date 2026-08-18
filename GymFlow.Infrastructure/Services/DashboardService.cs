using GymFlow.Application.Modules.Dashboard;
using GymFlow.Domain.Entities;
using GymFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GymFlow.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _db;
    public DashboardService(AppDbContext db) => _db = db;

    public async Task<DashboardStatsDto> GetStatsAsync(Guid gymId)
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var lastMonthStart = monthStart.AddMonths(-1);
        var today = DateOnly.FromDateTime(now);
        var soon = DateOnly.FromDateTime(now.AddDays(7));

        var totalMembers = await _db.Members.CountAsync(m => m.GymId == gymId && !m.IsDeleted);
        var activeMembers = await _db.Members.CountAsync(m => m.GymId == gymId && !m.IsDeleted && m.Status == MemberStatus.Active);
        var newThisMonth = await _db.Members.CountAsync(m => m.GymId == gymId && !m.IsDeleted && m.CreatedAt >= monthStart);
        var expiringSoon = await _db.Members.CountAsync(m => m.GymId == gymId && !m.IsDeleted
            && m.Status == MemberStatus.Active && m.ExpiryDate >= today && m.ExpiryDate <= soon);

        var monthlyRevenue = await _db.Transactions.Where(t => t.GymId == gymId && t.Status == PaymentStatus.Paid && t.PaidAt >= monthStart).SumAsync(t => t.Amount);
        var lastMonthRevenue = await _db.Transactions.Where(t => t.GymId == gymId && t.Status == PaymentStatus.Paid && t.PaidAt >= lastMonthStart && t.PaidAt < monthStart).SumAsync(t => t.Amount);
        var revenueGrowth = lastMonthRevenue > 0 ? ((monthlyRevenue - lastMonthRevenue) / lastMonthRevenue) * 100 : 0;

        var pending = await _db.Transactions.Where(t => t.GymId == gymId && t.Status == PaymentStatus.Pending).SumAsync(t => t.Amount);
        var totalProducts = await _db.Products.CountAsync(p => p.GymId == gymId && !p.IsDeleted);
        var lowStock = await _db.InventoryItems.CountAsync(i => i.GymId == gymId && !i.IsDeleted && i.Quantity <= i.MinQuantity);
        var totalStaff = await _db.Staff.CountAsync(s => s.GymId == gymId && !s.IsDeleted && s.IsActive);
        var todayCheckins = await _db.MemberCheckIns.CountAsync(c => c.GymId == gymId && c.CheckInTime.Date == now.Date);

        var gym = await _db.Gyms.FindAsync(gymId);
        var occupancy = gym?.Capacity > 0 ? (int)((double)todayCheckins / gym.Capacity * 100) : 0;

        return new DashboardStatsDto
        {
            TotalMembers = totalMembers, ActiveMembers = activeMembers,
            NewMembersThisMonth = newThisMonth, ExpiringSoon = expiringSoon,
            MonthlyRevenue = monthlyRevenue, RevenueGrowth = Math.Round(revenueGrowth, 1),
            PendingPayments = pending, TotalProducts = totalProducts, LowStockItems = lowStock,
            TotalStaff = totalStaff, TodayCheckIns = todayCheckins, OccupancyRate = Math.Min(100, occupancy),
        };
    }

    public async Task<List<RecentActivityDto>> GetRecentActivityAsync(Guid gymId)
    {
        var activities = new List<RecentActivityDto>();

        var txns = await _db.Transactions.Include(t => t.Member)
            .Where(t => t.GymId == gymId).OrderByDescending(t => t.CreatedAt).Take(4).ToListAsync();
        activities.AddRange(txns.Select(t => new RecentActivityDto(
            t.Id.ToString(), "payment",
            $"{t.Member?.FirstName ?? "Guest"} — {t.Description} ₹{t.Amount:N0}",
            GetRelativeTime(t.CreatedAt), "💳")));

        var members = await _db.Members.Where(m => m.GymId == gymId && !m.IsDeleted)
            .OrderByDescending(m => m.CreatedAt).Take(3).ToListAsync();
        activities.AddRange(members.Select(m => new RecentActivityDto(
            m.Id.ToString(), "new_member",
            $"{m.FirstName} {m.LastName} joined",
            GetRelativeTime(m.CreatedAt), "👤")));

        return activities.OrderByDescending(a => a.Time).Take(8).ToList();
    }

    public async Task<List<RevenueChartItem>> GetRevenueChartAsync(Guid gymId)
    {
        var result = new List<RevenueChartItem>();

        for (int i = 5; i >= 0; i--)
        {
            var date = DateTime.UtcNow.AddMonths(-i);
            var start = new DateTime(date.Year, date.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var end = start.AddMonths(1);

            var rev = await _db.Transactions
                .Where(t => t.GymId == gymId && t.Status == PaymentStatus.Paid && t.PaidAt >= start && t.PaidAt < end)
                .SumAsync(t => t.Amount);

            result.Add(new RevenueChartItem(date.ToString("MMM"), rev, rev * 0.35m));
        }

        return result;
    }

    public async Task<object> GetSummaryAsync(Guid gymId)
    {
        var byType = await _db.Transactions
            .Where(t => t.GymId == gymId && t.Status == PaymentStatus.Paid)
            .GroupBy(t => t.Type)
            .Select(g => new { type = g.Key.ToString(), amount = g.Sum(t => t.Amount) })
            .ToListAsync();

        return new { revenueByType = byType };
    }

    private static string GetRelativeTime(DateTime dt)
    {
        var diff = DateTime.UtcNow - dt;
        if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes} min ago";
        if (diff.TotalHours < 24) return $"{(int)diff.TotalHours} hr ago";
        return $"{(int)diff.TotalDays} day ago";
    }
}
