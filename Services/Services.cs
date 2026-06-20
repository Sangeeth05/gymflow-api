using GymFlow.API.Data;
using GymFlow.API.DTOs;
using GymFlow.API.Models;
using Microsoft.EntityFrameworkCore;

namespace GymFlow.API.Services;

// ── Member Service ────────────────────────────────────────────────────────────
public class MemberService : IMemberService
{
    private readonly AppDbContext _db;
    public MemberService(AppDbContext db) => _db = db;

    public async Task<PaginatedResponse<MemberDto>> GetAllAsync(Guid gymId, int page, int pageSize,
        string? search, string? status, string? sortBy, string? sortOrder)
    {
        var query = _db.Members
            .Include(m => m.MembershipPlan)
            .Where(m => m.GymId == gymId && !m.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var q = search.ToLower();
            query = query.Where(m =>
                m.FirstName.ToLower().Contains(q) ||
                m.LastName.ToLower().Contains(q) ||
                m.Email.ToLower().Contains(q) ||
                m.MemberId.ToLower().Contains(q));
        }

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<MemberStatus>(status, out var s))
            query = query.Where(m => m.Status == s);

        query = (sortBy?.ToLower(), sortOrder?.ToLower()) switch
        {
            ("firstname", "asc") => query.OrderBy(m => m.FirstName),
            ("firstname", _)     => query.OrderByDescending(m => m.FirstName),
            ("joindate", "asc")  => query.OrderBy(m => m.JoinDate),
            ("joindate", _)      => query.OrderByDescending(m => m.JoinDate),
            (_, "asc")           => query.OrderBy(m => m.CreatedAt),
            _                    => query.OrderByDescending(m => m.CreatedAt),
        };

        var total = await query.CountAsync();
        var members = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        var memberIds = members.Select(m => m.Id).ToList();
        var paymentTotals = await _db.Transactions
            .Where(t => t.MemberId.HasValue && memberIds.Contains(t.MemberId.Value) && t.Status == PaymentStatus.Paid)
            .GroupBy(t => t.MemberId!.Value)
            .Select(g => new { MemberId = g.Key, Total = g.Sum(t => t.Amount) })
            .ToDictionaryAsync(x => x.MemberId, x => x.Total);

        var lastVisits = await _db.MemberCheckIns
            .Where(c => memberIds.Contains(c.MemberId))
            .GroupBy(c => c.MemberId)
            .Select(g => new { MemberId = g.Key, Last = g.Max(c => c.CheckInTime) })
            .ToDictionaryAsync(x => x.MemberId, x => x.Last);

        return new PaginatedResponse<MemberDto>
        {
            Data = members.Select(m => MapToDto(m,
                paymentTotals.GetValueOrDefault(m.Id),
                lastVisits.TryGetValue(m.Id, out var lv) ? lv : null)).ToList(),
            Total = total, Page = page, PageSize = pageSize
        };
    }

    public async Task<MemberDto?> GetByIdAsync(Guid id, Guid gymId)
    {
        var member = await _db.Members.Include(m => m.MembershipPlan)
            .FirstOrDefaultAsync(m => m.Id == id && m.GymId == gymId && !m.IsDeleted);
        if (member == null) return null;
        var totalPaid = await _db.Transactions
            .Where(t => t.MemberId == id && t.Status == PaymentStatus.Paid)
            .SumAsync(t => t.Amount);
        var lastVisit = await _db.MemberCheckIns
            .Where(c => c.MemberId == id)
            .MaxAsync(c => (DateTime?)c.CheckInTime);
        return MapToDto(member, totalPaid, lastVisit);
    }

    public async Task<MemberDto> CreateAsync(CreateMemberDto dto, Guid gymId)
    {
        var plan = await _db.MembershipPlans.FindAsync(dto.MembershipPlanId)
            ?? throw new InvalidOperationException("Membership plan not found");

        // Generate member ID
        var count = await _db.Members.Where(m => m.GymId == gymId).CountAsync();
        var memberId = $"GF-{(count + 1):D4}";

        // Calculate expiry
        var joinDate = DateOnly.Parse(dto.JoinDate);
        var expiry = plan.BillingCycle switch
        {
            BillingCycle.Monthly    => joinDate.AddMonths(1),
            BillingCycle.Quarterly  => joinDate.AddMonths(3),
            BillingCycle.HalfYearly => joinDate.AddMonths(6),
            BillingCycle.Yearly     => joinDate.AddYears(1),
            _ => joinDate.AddMonths(1)
        };

        var member = new Member
        {
            MemberId = memberId,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            Gender = Enum.Parse<Gender>(dto.Gender),
            DateOfBirth = DateOnly.Parse(dto.DateOfBirth),
            Address = dto.Address,
            City = dto.City,
            MembershipPlanId = dto.MembershipPlanId,
            JoinDate = joinDate,
            ExpiryDate = expiry,
            EmergencyContact = dto.EmergencyContact,
            EmergencyPhone = dto.EmergencyPhone,
            Notes = dto.Notes,
            GymId = gymId,
            Status = MemberStatus.Active,
        };

        _db.Members.Add(member);
        await _db.SaveChangesAsync();

        member.MembershipPlan = plan;
        return MapToDto(member, 0, null);
    }

    public async Task<MemberDto?> UpdateAsync(Guid id, UpdateMemberDto dto, Guid gymId)
    {
        var member = await _db.Members.Include(m => m.MembershipPlan)
            .FirstOrDefaultAsync(m => m.Id == id && m.GymId == gymId && !m.IsDeleted);
        if (member == null) return null;

        if (dto.FirstName != null) member.FirstName = dto.FirstName;
        if (dto.LastName != null) member.LastName = dto.LastName;
        if (dto.Email != null) member.Email = dto.Email;
        if (dto.Phone != null) member.Phone = dto.Phone;
        if (dto.Address != null) member.Address = dto.Address;
        if (dto.City != null) member.City = dto.City;
        if (dto.Notes != null) member.Notes = dto.Notes;
        if (dto.EmergencyContact != null) member.EmergencyContact = dto.EmergencyContact;
        if (dto.EmergencyPhone != null) member.EmergencyPhone = dto.EmergencyPhone;
        if (dto.Status != null && Enum.TryParse<MemberStatus>(dto.Status, out var s))
            member.Status = s;
        if (dto.MembershipPlanId.HasValue)
        {
            var plan = await _db.MembershipPlans.FindAsync(dto.MembershipPlanId.Value);
            if (plan != null) { member.MembershipPlanId = plan.Id; member.MembershipPlan = plan; }
        }

        member.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return MapToDto(member, 0, null);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid gymId)
    {
        var member = await _db.Members.FirstOrDefaultAsync(m => m.Id == id && m.GymId == gymId && !m.IsDeleted);
        if (member == null) return false;
        member.IsDeleted = true;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task CheckInAsync(Guid memberId, Guid gymId)
    {
        var checkIn = new MemberCheckIn { MemberId = memberId, GymId = gymId };
        _db.MemberCheckIns.Add(checkIn);
        await _db.SaveChangesAsync();
    }

    public async Task<List<MemberDto>> GetExpiringAsync(Guid gymId, int days)
    {
        var cutoff = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(days));
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var members = await _db.Members.Include(m => m.MembershipPlan)
            .Where(m => m.GymId == gymId && !m.IsDeleted && m.Status == MemberStatus.Active
                && m.ExpiryDate >= today && m.ExpiryDate <= cutoff)
            .ToListAsync();
        return members.Select(m => MapToDto(m, 0, null)).ToList();
    }

    public async Task<List<object>> GetAllPlansAsync(Guid gymId)
    {
        var plans = await _db.MembershipPlans
            .Where(p => p.GymId == gymId && !p.IsDeleted)
            .Select(p => new
            {
                id = p.Id.ToString(),
                p.Name,
                p.Description,
                p.Price,
                billingCycle = p.BillingCycle.ToString(),
                p.IsActive,
                p.Color,
                currentMembers = p.Members.Count(m => !m.IsDeleted),
                createdAt = p.CreatedAt,
            })
            .ToListAsync();
        return plans.Cast<object>().ToList();
    }

    private static MemberDto MapToDto(Member m, decimal totalPaid, DateTime? lastVisit) => new()
    {
        Id = m.Id,
        MemberId = m.MemberId,
        FirstName = m.FirstName,
        LastName = m.LastName,
        Email = m.Email,
        Phone = m.Phone,
        Gender = m.Gender.ToString(),
        DateOfBirth = m.DateOfBirth.ToString("yyyy-MM-dd"),
        Address = m.Address,
        City = m.City,
        Status = m.Status.ToString(),
        MembershipPlanId = m.MembershipPlanId,
        MembershipPlanName = m.MembershipPlan?.Name ?? "",
        JoinDate = m.JoinDate.ToString("yyyy-MM-dd"),
        ExpiryDate = m.ExpiryDate.ToString("yyyy-MM-dd"),
        EmergencyContact = m.EmergencyContact,
        EmergencyPhone = m.EmergencyPhone,
        Notes = m.Notes,
        TotalPayments = totalPaid,
        LastVisit = lastVisit,
        CreatedAt = m.CreatedAt,
    };
}

// ── Dashboard Service ─────────────────────────────────────────────────────────
public class DashboardService : IDashboardService
{
    private readonly AppDbContext _db;
    public DashboardService(AppDbContext db) => _db = db;

    public async Task<DashboardStatsDto> GetStatsAsync(Guid gymId)
    {
        //var now = DateTime.UtcNow;
        //var monthStart = new DateTime(now.Year, now.Month, 1);
        //var lastMonthStart = monthStart.AddMonths(-1);
        var now = DateTime.UtcNow;

        var monthStart = new DateTime(
            now.Year,
            now.Month,
            1,
            0,
            0,
            0,
            DateTimeKind.Utc);

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

        // Recent transactions
        var txns = await _db.Transactions.Include(t => t.Member)
            .Where(t => t.GymId == gymId).OrderByDescending(t => t.CreatedAt).Take(4).ToListAsync();
        activities.AddRange(txns.Select(t => new RecentActivityDto(
            t.Id.ToString(), "payment",
            $"{t.Member?.FirstName ?? "Guest"} — {t.Description} ₹{t.Amount:N0}",
            GetRelativeTime(t.CreatedAt), "💳")));

        // Recent members
        var members = await _db.Members.Where(m => m.GymId == gymId && !m.IsDeleted)
            .OrderByDescending(m => m.CreatedAt).Take(3).ToListAsync();
        activities.AddRange(members.Select(m => new RecentActivityDto(
            m.Id.ToString(), "new_member",
            $"{m.FirstName} {m.LastName} joined",
            GetRelativeTime(m.CreatedAt), "👤")));

        return activities.OrderByDescending(a => a.Time).Take(8).ToList();
    }

    //public async Task<List<RevenueChartItem>> GetRevenueChartAsync(Guid gymId)
    //{
    //    var result = new List<RevenueChartItem>();
    //    for (int i = 5; i >= 0; i--)
    //    {
    //        var date = DateTime.UtcNow.AddMonths(-i);
    //        var start = new DateTime(date.Year, date.Month, 1);
    //        var end = start.AddMonths(1);
    //        var rev = await _db.Transactions.Where(t => t.GymId == gymId && t.Status == PaymentStatus.Paid
    //            && t.PaidAt >= start && t.PaidAt < end).SumAsync(t => t.Amount);
    //        result.Add(new RevenueChartItem(date.ToString("MMM"), rev, rev * 0.35m)); // mock expenses
    //    }
    //    return result;
    //}
    public async Task<List<RevenueChartItem>> GetRevenueChartAsync(Guid gymId)
    {
        var result = new List<RevenueChartItem>();

        for (int i = 5; i >= 0; i--)
        {
            var date = DateTime.UtcNow.AddMonths(-i);

            var start = new DateTime(
                date.Year,
                date.Month,
                1,
                0,
                0,
                0,
                DateTimeKind.Utc);

            var end = start.AddMonths(1);

            var rev = await _db.Transactions
                .Where(t =>
                    t.GymId == gymId &&
                    t.Status == PaymentStatus.Paid &&
                    t.PaidAt >= start &&
                    t.PaidAt < end)
                .SumAsync(t => t.Amount);

            result.Add(new RevenueChartItem(
                date.ToString("MMM"),   
                rev,
                rev * 0.35m));
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

// ── Stub Services ─────────────────────────────────────────────────────────────
// These are basic pass-through stubs — expand each with full EF logic
public class FinanceService : IFinanceService
{
    private readonly AppDbContext _db;
    public FinanceService(AppDbContext db) => _db = db;

    public async Task<PaginatedResponse<object>> GetTransactionsAsync(Guid gymId, int page, int pageSize, string? search, string? status)
    {
        var query = _db.Transactions.Include(t => t.Member)
            .Where(t => t.GymId == gymId).AsQueryable();
        if (!string.IsNullOrEmpty(search))
        {
            var q = search.ToLower();
            query = query.Where(t => t.TransactionId.ToLower().Contains(q) || t.Description.ToLower().Contains(q));
        }
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<PaymentStatus>(status, out var s))
            query = query.Where(t => t.Status == s);
        var total = await query.CountAsync();
        var data = await query.OrderByDescending(t => t.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(t => (object)new {
                id = t.Id.ToString(), transactionId = t.TransactionId,
                memberId = t.MemberId.HasValue ? t.MemberId.Value.ToString() : null,
                memberName = t.Member != null ? t.Member.FirstName + " " + t.Member.LastName : null,
                type = t.Type.ToString(), t.Description, t.Amount,
                paymentMethod = t.PaymentMethod.ToString(), status = t.Status.ToString(),
                t.DueDate, t.PaidAt, t.CreatedAt
            }).ToListAsync();
        return new PaginatedResponse<object> { Data = data, Total = total, Page = page, PageSize = pageSize };
    }

    public async Task<object> GetSummaryAsync(Guid gymId)
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0,0,0,DateTimeKind.Utc);
        var total = await _db.Transactions.Where(t => t.GymId == gymId && t.Status == PaymentStatus.Paid).SumAsync(t => t.Amount);
        var monthly = await _db.Transactions.Where(t => t.GymId == gymId && t.Status == PaymentStatus.Paid && t.PaidAt >= monthStart).SumAsync(t => t.Amount);
        var pending = await _db.Transactions.Where(t => t.GymId == gymId && t.Status == PaymentStatus.Pending).SumAsync(t => t.Amount);
        var overdue = await _db.Transactions.Where(t => t.GymId == gymId && t.Status == PaymentStatus.Overdue).SumAsync(t => t.Amount);
        return new { totalRevenue = total, monthlyRevenue = monthly, pendingPayments = pending, overduePayments = overdue };
    }

    public Task<object> CreateTransactionAsync(Guid gymId, object dto) =>
        Task.FromResult<object>(new { message = "Transaction created" });
}

public class InventoryService : IInventoryService
{
    private readonly AppDbContext _db;
    public InventoryService(AppDbContext db) => _db = db;

    public async Task<PaginatedResponse<object>> GetAllAsync(Guid gymId, int page, int pageSize, string? search, string? category)
    {
        var query = _db.InventoryItems.Where(i => i.GymId == gymId && !i.IsDeleted).AsQueryable();
        if (!string.IsNullOrEmpty(search)) { var q = search.ToLower(); query = query.Where(i => i.Name.ToLower().Contains(q) || i.Sku.ToLower().Contains(q)); }
        if (!string.IsNullOrEmpty(category)) query = query.Where(i => i.Category == category);
        var total = await query.CountAsync();
        var data = await query.OrderBy(i => i.Name).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(i => (object)new {
                id = i.Id.ToString(), i.Sku, i.Name, i.Category, i.Description,
                i.Quantity, i.MinQuantity, i.Unit, i.PurchasePrice, i.SellingPrice,
                i.Supplier, i.Location, i.LastRestocked, i.CreatedAt,
                status = i.Quantity == 0 ? "OutOfStock" : i.Quantity <= i.MinQuantity ? "LowStock" : "InStock"
            }).ToListAsync();
        return new PaginatedResponse<object> { Data = data, Total = total, Page = page, PageSize = pageSize };
    }

    public Task<object?> GetByIdAsync(Guid id, Guid gymId) => Task.FromResult<object?>(null);
    public Task<object> CreateAsync(Guid gymId, object dto) => Task.FromResult<object>(new { });
    public Task<object?> UpdateAsync(Guid id, Guid gymId, object dto) => Task.FromResult<object?>(null);
    public async Task<bool> DeleteAsync(Guid id, Guid gymId)
    {
        var item = await _db.InventoryItems.FirstOrDefaultAsync(i => i.Id == id && i.GymId == gymId);
        if (item == null) return false;
        item.IsDeleted = true;
        await _db.SaveChangesAsync();
        return true;
    }
}

public class ProductService : IProductService
{
    private readonly AppDbContext _db;
    public ProductService(AppDbContext db) => _db = db;

    public async Task<PaginatedResponse<object>> GetAllAsync(Guid gymId, int page, int pageSize, string? search, string? category)
    {
        var query = _db.Products.Where(p => p.GymId == gymId && !p.IsDeleted).AsQueryable();
        if (!string.IsNullOrEmpty(search)) { var q = search.ToLower(); query = query.Where(p => p.Name.ToLower().Contains(q) || p.Brand.ToLower().Contains(q)); }
        var total = await query.CountAsync();
        var data = await query.OrderByDescending(p => p.IsFeatured).ThenBy(p => p.Name)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(p => (object)new { id = p.Id.ToString(), p.Name, p.Brand, p.Category, p.Description, p.Price, p.OriginalPrice, p.Stock, p.Sku, p.IsActive, p.IsFeatured, p.Rating, p.ReviewCount, p.CreatedAt }).ToListAsync();
        return new PaginatedResponse<object> { Data = data, Total = total, Page = page, PageSize = pageSize };
    }

    public Task<object> CreateAsync(Guid gymId, object dto) => Task.FromResult<object>(new { });
    public Task<object?> UpdateAsync(Guid id, Guid gymId, object dto) => Task.FromResult<object?>(null);
    public async Task<bool> DeleteAsync(Guid id, Guid gymId)
    {
        var p = await _db.Products.FirstOrDefaultAsync(x => x.Id == id && x.GymId == gymId);
        if (p == null) return false;
        p.IsDeleted = true;
        await _db.SaveChangesAsync();
        return true;
    }
}

public class PromoCodeService : IPromoCodeService
{
    private readonly AppDbContext _db;
    public PromoCodeService(AppDbContext db) => _db = db;

    public async Task<List<object>> GetAllAsync(Guid gymId)
    {
        var now = DateTime.UtcNow;
        return await _db.PromoCodes.Where(p => p.GymId == gymId && !p.IsDeleted)
            .Select(p => (object)new {
                id = p.Id.ToString(), p.Code, p.Description,
                discountType = p.DiscountType.ToString(), p.DiscountValue, p.MinPurchase, p.MaxUses, p.UsedCount,
                validFrom = p.ValidFrom, validTo = p.ValidTo,
                applicableFor = p.ApplicableFor.ToString(),
                status = now < p.ValidFrom ? "Scheduled" : now > p.ValidTo ? "Expired" : p.UsedCount >= p.MaxUses ? "Inactive" : "Active",
                p.CreatedAt
            }).ToListAsync();
    }

    public Task<object> CreateAsync(Guid gymId, object dto) => Task.FromResult<object>(new { });
    public Task<object?> UpdateAsync(Guid id, Guid gymId, object dto) => Task.FromResult<object?>(null);
    public async Task<bool> DeleteAsync(Guid id, Guid gymId)
    {
        var p = await _db.PromoCodes.FirstOrDefaultAsync(x => x.Id == id && x.GymId == gymId);
        if (p == null) return false;
        p.IsDeleted = true;
        await _db.SaveChangesAsync();
        return true;
    }
    public Task<object> ValidateAsync(string code, Guid gymId, decimal amount) =>
        Task.FromResult<object>(new { valid = false, message = "Code not found" });
}

public class StaffService : IStaffService
{
    private readonly AppDbContext _db;
    public StaffService(AppDbContext db) => _db = db;

    public async Task<List<object>> GetAllAsync(Guid gymId, string? role)
    {
        var query = _db.Staff.Where(s => s.GymId == gymId && !s.IsDeleted).AsQueryable();
        if (!string.IsNullOrEmpty(role) && Enum.TryParse<StaffRole>(role, out var r))
            query = query.Where(s => s.Role == r);
        return await query.Select(s => (object)new {
            id = s.Id.ToString(), s.StaffId, s.FirstName, s.LastName, s.Email, s.Phone,
            role = s.Role.ToString(), s.Salary, joinDate = s.JoinDate.ToString("yyyy-MM-dd"),
            status = s.IsActive ? "Active" : "Inactive", s.CreatedAt
        }).ToListAsync();
    }

    public Task<object?> GetByIdAsync(Guid id, Guid gymId) => Task.FromResult<object?>(null);
    public Task<object> CreateAsync(Guid gymId, object dto) => Task.FromResult<object>(new { });
    public Task<object?> UpdateAsync(Guid id, Guid gymId, object dto) => Task.FromResult<object?>(null);
    public async Task<bool> DeleteAsync(Guid id, Guid gymId)
    {
        var s = await _db.Staff.FirstOrDefaultAsync(x => x.Id == id && x.GymId == gymId);
        if (s == null) return false;
        s.IsDeleted = true;
        await _db.SaveChangesAsync();
        return true;
    }
}
