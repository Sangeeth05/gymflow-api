using GymFlow.Application.Common;
using GymFlow.Application.Modules.Members;
using GymFlow.Domain.Entities;
using GymFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GymFlow.Infrastructure.Services;

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

        var count = await _db.Members.Where(m => m.GymId == gymId).CountAsync();
        var memberId = $"GF-{(count + 1):D4}";

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
