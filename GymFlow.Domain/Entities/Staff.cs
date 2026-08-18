using System.ComponentModel.DataAnnotations;
using GymFlow.Domain.Common;

namespace GymFlow.Domain.Entities;

public enum StaffRole { Trainer, Receptionist, Manager, Cleaner, Security }

public class Staff : BaseEntity
{
    [Required, MaxLength(20)]  public string StaffId { get; set; } = "";
    [Required, MaxLength(100)] public string FirstName { get; set; } = "";
    [Required, MaxLength(100)] public string LastName { get; set; } = "";
    [Required, MaxLength(200)] public string Email { get; set; } = "";
    [MaxLength(20)]  public string Phone { get; set; } = "";
    public StaffRole Role { get; set; }
    public decimal Salary { get; set; }
    public DateOnly JoinDate { get; set; }
    public bool IsActive { get; set; } = true;
    public string Specializations { get; set; } = "[]"; // JSON array
    [MaxLength(200)] public string? Schedule { get; set; }
    [MaxLength(500)] public string? ProfilePhoto { get; set; }
    public Guid GymId { get; set; }
}
