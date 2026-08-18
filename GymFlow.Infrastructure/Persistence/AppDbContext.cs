using GymFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymFlow.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Gym> Gyms => Set<Gym>();
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<MembershipPlan> MembershipPlans => Set<MembershipPlan>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<PromoCode> PromoCodes => Set<PromoCode>();
    public DbSet<Staff> Staff => Set<Staff>();
    public DbSet<MemberCheckIn> MemberCheckIns => Set<MemberCheckIn>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User (unified identity)
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasOne(u => u.Gym)
            .WithMany(g => g.Users)
            .HasForeignKey(u => u.GymId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        modelBuilder.Entity<User>()
            .HasOne(u => u.Staff)
            .WithMany()
            .HasForeignKey(u => u.StaffId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        modelBuilder.Entity<User>()
            .HasOne(u => u.Member)
            .WithMany()
            .HasForeignKey(u => u.MemberId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        modelBuilder.Entity<RefreshToken>()
            .HasOne(t => t.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Member
        modelBuilder.Entity<Member>()
            .HasIndex(m => m.MemberId)
            .IsUnique();

        modelBuilder.Entity<Member>()
            .HasIndex(m => m.Email)
            .IsUnique();

        modelBuilder.Entity<Member>()
            .HasOne(m => m.MembershipPlan)
            .WithMany(p => p.Members)
            .HasForeignKey(m => m.MembershipPlanId)
            .OnDelete(DeleteBehavior.Restrict);

        // Transaction
        modelBuilder.Entity<Transaction>()
            .Property(t => t.Amount)
            .HasPrecision(18, 2);

        // Product
        modelBuilder.Entity<Product>()
            .Property(p => p.Price)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Product>()
            .Property(p => p.OriginalPrice)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Product>()
            .Property(p => p.Rating)
            .HasPrecision(18, 2);

        // InventoryItem
        modelBuilder.Entity<InventoryItem>()
            .Property(i => i.PurchasePrice)
            .HasPrecision(18, 2);

        modelBuilder.Entity<InventoryItem>()
            .Property(i => i.SellingPrice)
            .HasPrecision(18, 2);

        // MembershipPlan
        modelBuilder.Entity<MembershipPlan>()
            .Property(p => p.Price)
            .HasPrecision(18, 2);

        // Staff
        modelBuilder.Entity<Staff>()
            .Property(s => s.Salary)
            .HasPrecision(18, 2);

        // PromoCode
        modelBuilder.Entity<PromoCode>()
            .Property(p => p.DiscountValue)
            .HasPrecision(18, 2);

        modelBuilder.Entity<PromoCode>()
            .Property(p => p.MinPurchase)
            .HasPrecision(18, 2);
    }
}
