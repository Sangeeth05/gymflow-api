using System.ComponentModel.DataAnnotations;

namespace GymFlow.API.DTOs;

// ── Auth ──────────────────────────────────────────────────────────────────────
public record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required, MinLength(6)] string Password
);

public record RefreshTokenRequest([Required] string RefreshToken);

public record AuthResponse(
    string Token,
    string RefreshToken,
    UserDto User
);

public record UserDto(
    Guid Id,
    string Name,
    string Email,
    string Role,
    Guid GymId,
    string GymName
);

// ── Member ────────────────────────────────────────────────────────────────────
public class CreateMemberDto
{
    [Required, MaxLength(100)] public string FirstName { get; set; } = "";
    [Required, MaxLength(100)] public string LastName { get; set; } = "";
    [Required, EmailAddress]   public string Email { get; set; } = "";
    [Required]                 public string Phone { get; set; } = "";
    [Required]                 public string Gender { get; set; } = "Male";
    [Required]                 public string DateOfBirth { get; set; } = "";
    [Required]                 public string Address { get; set; } = "";
    [Required]                 public string City { get; set; } = "";
    [Required]                 public Guid MembershipPlanId { get; set; }
    [Required]                 public string JoinDate { get; set; } = "";
    [Required]                 public string EmergencyContact { get; set; } = "";
    [Required]                 public string EmergencyPhone { get; set; } = "";
    public string? Notes { get; set; }
}

public class UpdateMemberDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Gender { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public Guid? MembershipPlanId { get; set; }
    public string? Status { get; set; }
    public string? Notes { get; set; }
    public string? EmergencyContact { get; set; }
    public string? EmergencyPhone { get; set; }
}

public class MemberDto
{
    public Guid Id { get; set; }
    public string MemberId { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Gender { get; set; } = "";
    public string DateOfBirth { get; set; } = "";
    public string Address { get; set; } = "";
    public string City { get; set; } = "";
    public string Status { get; set; } = "";
    public Guid MembershipPlanId { get; set; }
    public string MembershipPlanName { get; set; } = "";
    public string JoinDate { get; set; } = "";
    public string ExpiryDate { get; set; } = "";
    public string EmergencyContact { get; set; } = "";
    public string EmergencyPhone { get; set; } = "";
    public string? Notes { get; set; }
    public decimal TotalPayments { get; set; }
    public DateTime? LastVisit { get; set; }
    public DateTime CreatedAt { get; set; }
}

// ── Pagination ────────────────────────────────────────────────────────────────
public class PaginatedResponse<T>
{
    public List<T> Data { get; set; } = [];
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)Total / PageSize);
}

// ── Dashboard ─────────────────────────────────────────────────────────────────
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
