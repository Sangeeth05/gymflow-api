using GymFlow.Application.Common;
using GymFlow.Application.Modules.Finance;
using GymFlow.Application.Modules.Inventory;
using GymFlow.Application.Modules.Products;
using GymFlow.Application.Modules.Promotions;
using GymFlow.Application.Modules.Staff;
using GymFlow.Domain.Entities;
using GymFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GymFlow.Infrastructure.Services;

// These are basic pass-through stubs, carried over as-is from the original
// single-project version — expand each with full EF logic as those modules
// get built out.

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
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
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
