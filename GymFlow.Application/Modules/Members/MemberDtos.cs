using System.ComponentModel.DataAnnotations;

namespace GymFlow.Application.Modules.Members;

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
