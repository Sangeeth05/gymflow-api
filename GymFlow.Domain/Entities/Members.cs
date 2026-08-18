using System.ComponentModel.DataAnnotations;
using GymFlow.Domain.Common;

namespace GymFlow.Domain.Entities;

// ── Membership Plan ───────────────────────────────────────────────────────────
public enum BillingCycle { Monthly, Quarterly, HalfYearly, Yearly }

public class MembershipPlan : BaseEntity
{
    [Required, MaxLength(100)] public string Name { get; set; } = "";
    [MaxLength(500)] public string Description { get; set; } = "";
    public decimal Price { get; set; }
    public BillingCycle BillingCycle { get; set; } = BillingCycle.Monthly;
    public string Features { get; set; } = "[]"; // JSON array
    public bool IsActive { get; set; } = true;
    [MaxLength(20)] public string Color { get; set; } = "#f97316";
    public int? MaxMembers { get; set; }
    public Guid GymId { get; set; }
    public ICollection<Member> Members { get; set; } = [];
}

// ── Member ───────────────────────────────────────────────────────────────────
public enum MemberStatus { Active, Inactive, Suspended, Expired }
public enum Gender { Male, Female, Other }

public class Member : BaseEntity
{
    [Required, MaxLength(20)]  public string MemberId { get; set; } = "";
    [Required, MaxLength(100)] public string FirstName { get; set; } = "";
    [Required, MaxLength(100)] public string LastName { get; set; } = "";
    [Required, MaxLength(200)] public string Email { get; set; } = "";
    [MaxLength(20)]  public string Phone { get; set; } = "";
    public Gender Gender { get; set; } = Gender.Male;
    public DateOnly DateOfBirth { get; set; }
    [MaxLength(500)] public string Address { get; set; } = "";
    [MaxLength(100)] public string City { get; set; } = "";
    [MaxLength(500)] public string? ProfilePhoto { get; set; }
    public MemberStatus Status { get; set; } = MemberStatus.Active;
    public Guid MembershipPlanId { get; set; }
    public MembershipPlan MembershipPlan { get; set; } = null!;
    public DateOnly JoinDate { get; set; }
    public DateOnly ExpiryDate { get; set; }
    [MaxLength(200)] public string EmergencyContact { get; set; } = "";
    [MaxLength(20)]  public string EmergencyPhone { get; set; } = "";
    [MaxLength(1000)] public string? Notes { get; set; }
    public Guid GymId { get; set; }
    public Gym Gym { get; set; } = null!;
    public ICollection<Transaction> Transactions { get; set; } = [];
    public ICollection<MemberCheckIn> CheckIns { get; set; } = [];
}

// ── Member Check-In ───────────────────────────────────────────────────────────
public class MemberCheckIn : BaseEntity
{
    public Guid MemberId { get; set; }
    public Member Member { get; set; } = null!;
    public DateTime CheckInTime { get; set; } = DateTime.UtcNow;
    public DateTime? CheckOutTime { get; set; }
    public Guid GymId { get; set; }
}
