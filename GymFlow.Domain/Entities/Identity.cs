using System.ComponentModel.DataAnnotations;
using GymFlow.Domain.Common;

namespace GymFlow.Domain.Entities;

// ── Gym ──────────────────────────────────────────────────────────────────────
public enum GymStatus { PendingApproval, Active, Suspended }

public class Gym : BaseEntity
{
    [Required, MaxLength(200)] public string Name { get; set; } = "";
    [MaxLength(500)] public string Address { get; set; } = "";
    [MaxLength(100)] public string City { get; set; } = "";
    [MaxLength(100)] public string Country { get; set; } = "";
    [MaxLength(20)]  public string Phone { get; set; } = "";
    [MaxLength(200)] public string Email { get; set; } = "";
    [MaxLength(200)] public string? Logo { get; set; }
    [MaxLength(100)] public string OpeningHours { get; set; } = "06:00-22:00";
    public int Capacity { get; set; } = 100;

    // New self-signup gyms start PendingApproval and cannot log in until a
    // SuperAdmin activates them.
    public GymStatus Status { get; set; } = GymStatus.PendingApproval;

    public ICollection<User> Users { get; set; } = [];
    public ICollection<Member> Members { get; set; } = [];
}

// ── User (unified identity for ALL logins) ────────────────────────────────────
public enum UserRole { SuperAdmin, GymAdmin, Trainer, Member }

// One login table for every role. GymId is null ONLY for SuperAdmin.
// StaffId is set when Role == Trainer, linking back to the HR/Staff record.
// MemberId is set when Role == Member, linking back to the membership profile.
public class User : BaseEntity
{
    [Required, MaxLength(200)] public string Name { get; set; } = "";
    [Required, MaxLength(200)] public string Email { get; set; } = "";
    [Required] public string PasswordHash { get; set; } = "";
    public UserRole Role { get; set; } = UserRole.GymAdmin;

    public Guid? GymId { get; set; }
    public Gym? Gym { get; set; }

    public Guid? StaffId { get; set; }
    public Staff? Staff { get; set; }

    public Guid? MemberId { get; set; }
    public Member? Member { get; set; }

    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}

public class RefreshToken : BaseEntity
{
    [Required] public string Token { get; set; } = "";
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; } = false;
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
}
