using System.ComponentModel.DataAnnotations;

namespace GymFlow.API.Models;

// ── Base ─────────────────────────────────────────────────────────────────────
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;
}

// ── Gym ──────────────────────────────────────────────────────────────────────
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
    public ICollection<AdminUser> AdminUsers { get; set; } = [];
    public ICollection<Member> Members { get; set; } = [];
}

// ── Admin User ────────────────────────────────────────────────────────────────
public enum AdminRole { SuperAdmin, Admin, Staff }

public class AdminUser : BaseEntity
{
    [Required, MaxLength(200)] public string Name { get; set; } = "";
    [Required, MaxLength(200)] public string Email { get; set; } = "";
    [Required] public string PasswordHash { get; set; } = "";
    public AdminRole Role { get; set; } = AdminRole.Admin;
    public Guid GymId { get; set; }
    public Gym Gym { get; set; } = null!;
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}

public class RefreshToken : BaseEntity
{
    [Required] public string Token { get; set; } = "";
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; } = false;
    public Guid AdminUserId { get; set; }
    public AdminUser AdminUser { get; set; } = null!;
}

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

// ── Transaction ───────────────────────────────────────────────────────────────
public enum PaymentStatus { Paid, Pending, Overdue, Refunded }
public enum PaymentMethod { Cash, Card, UPI, BankTransfer, Cheque }
public enum TransactionType { MembershipFee, ProductSale, PersonalTraining, LockerRental, Other }

public class Transaction : BaseEntity
{
    [Required, MaxLength(50)] public string TransactionId { get; set; } = "";
    public Guid? MemberId { get; set; }
    public Member? Member { get; set; }
    public TransactionType Type { get; set; }
    [MaxLength(500)] public string Description { get; set; } = "";
    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public DateTime? DueDate { get; set; }
    public DateTime? PaidAt { get; set; }
    [MaxLength(500)] public string? InvoiceUrl { get; set; }
    public Guid GymId { get; set; }
}

// ── Inventory ─────────────────────────────────────────────────────────────────
public enum StockStatus { InStock, LowStock, OutOfStock }

public class InventoryItem : BaseEntity
{
    [Required, MaxLength(50)]  public string Sku { get; set; } = "";
    [Required, MaxLength(200)] public string Name { get; set; } = "";
    [MaxLength(100)] public string Category { get; set; } = "";
    [MaxLength(500)] public string Description { get; set; } = "";
    public int Quantity { get; set; }
    public int MinQuantity { get; set; } = 5;
    [MaxLength(50)] public string Unit { get; set; } = "units";
    public decimal PurchasePrice { get; set; }
    public decimal SellingPrice { get; set; }
    [MaxLength(200)] public string Supplier { get; set; } = "";
    [MaxLength(200)] public string Location { get; set; } = "";
    public DateTime? LastRestocked { get; set; }
    [MaxLength(500)] public string? Image { get; set; }
    public Guid GymId { get; set; }

    public StockStatus Status =>
        Quantity == 0 ? StockStatus.OutOfStock :
        Quantity <= MinQuantity ? StockStatus.LowStock : StockStatus.InStock;
}

// ── Product ───────────────────────────────────────────────────────────────────
public class Product : BaseEntity
{
    [Required, MaxLength(200)] public string Name { get; set; } = "";
    [MaxLength(200)] public string Brand { get; set; } = "";
    [MaxLength(100)] public string Category { get; set; } = "";
    [MaxLength(1000)] public string Description { get; set; } = "";
    public decimal Price { get; set; }
    public decimal? OriginalPrice { get; set; }
    public int Stock { get; set; }
    public string Images { get; set; } = "[]"; // JSON array
    [Required, MaxLength(50)] public string Sku { get; set; } = "";
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; } = false;
    public decimal Rating { get; set; } = 0;
    public int ReviewCount { get; set; } = 0;
    public string Tags { get; set; } = "[]"; // JSON array
    public Guid GymId { get; set; }
}

// ── Promo Code ────────────────────────────────────────────────────────────────
public enum DiscountType { Percentage, FixedAmount }
public enum PromoStatus { Active, Inactive, Expired, Scheduled }
public enum PromoApplicableFor { All, NewMembers, RenewalOnly, ProductsOnly }

public class PromoCode : BaseEntity
{
    [Required, MaxLength(50)] public string Code { get; set; } = "";
    [MaxLength(500)] public string Description { get; set; } = "";
    public DiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public decimal MinPurchase { get; set; }
    public int MaxUses { get; set; }
    public int UsedCount { get; set; } = 0;
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
    public PromoApplicableFor ApplicableFor { get; set; } = PromoApplicableFor.All;
    public Guid GymId { get; set; }

    public PromoStatus Status =>
        DateTime.UtcNow < ValidFrom ? PromoStatus.Scheduled :
        DateTime.UtcNow > ValidTo ? PromoStatus.Expired :
        UsedCount >= MaxUses ? PromoStatus.Inactive : PromoStatus.Active;
}

// ── Staff ─────────────────────────────────────────────────────────────────────
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

// ── Member Check-In ───────────────────────────────────────────────────────────
public class MemberCheckIn : BaseEntity
{
    public Guid MemberId { get; set; }
    public Member Member { get; set; } = null!;
    public DateTime CheckInTime { get; set; } = DateTime.UtcNow;
    public DateTime? CheckOutTime { get; set; }
    public Guid GymId { get; set; }
}
