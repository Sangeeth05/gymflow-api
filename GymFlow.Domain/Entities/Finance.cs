using System.ComponentModel.DataAnnotations;
using GymFlow.Domain.Common;

namespace GymFlow.Domain.Entities;

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
