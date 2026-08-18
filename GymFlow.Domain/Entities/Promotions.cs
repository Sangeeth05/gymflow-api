using System.ComponentModel.DataAnnotations;
using GymFlow.Domain.Common;

namespace GymFlow.Domain.Entities;

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
