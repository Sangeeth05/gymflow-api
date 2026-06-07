using GymFlow.API.Models;
using Microsoft.EntityFrameworkCore;

namespace GymFlow.API.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        // Only seed if no gyms exist
        if (await db.Gyms.AnyAsync()) return;

        // ── Gym ───────────────────────────────────────────────────────────────
        var gym = new Gym
        {
            Id = Guid.NewGuid(),
            Name = "GymFlow Fitness Center",
            Address = "42 MG Road, Pattom",
            City = "Thiruvananthapuram",
            Country = "India",
            Phone = "+91 471 2345678",
            Email = "info@gymflow.com",
            OpeningHours = "05:30-22:00",
            Capacity = 200,
        };
        db.Gyms.Add(gym);

        // ── Admin User ────────────────────────────────────────────────────────
        var admin = new AdminUser
        {
            Id = Guid.NewGuid(),
            Name = "Admin User",
            Email = "admin@gymflow.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            Role = AdminRole.Admin,
            GymId = gym.Id,
        };
        db.AdminUsers.Add(admin);

        // ── Membership Plans ──────────────────────────────────────────────────
        var plans = new List<MembershipPlan>
        {
            new() { Id = Guid.NewGuid(), Name = "Basic", Description = "Perfect for beginners", Price = 999, BillingCycle = BillingCycle.Monthly, Features = "[\"Gym Access\",\"Locker Room\",\"Basic Equipment\"]", IsActive = true, Color = "#3b82f6", GymId = gym.Id },
            new() { Id = Guid.NewGuid(), Name = "Premium", Description = "Most popular plan", Price = 1999, BillingCycle = BillingCycle.Monthly, Features = "[\"All Basic Features\",\"Group Classes\",\"Sauna\",\"1 PT Session/Month\"]", IsActive = true, Color = "#f97316", GymId = gym.Id },
            new() { Id = Guid.NewGuid(), Name = "Gold", Description = "Quarterly commitment", Price = 2999, BillingCycle = BillingCycle.Quarterly, Features = "[\"All Premium Features\",\"4 PT Sessions\",\"Nutrition Consultation\",\"Body Analysis\"]", IsActive = true, Color = "#eab308", GymId = gym.Id },
            new() { Id = Guid.NewGuid(), Name = "Elite Annual", Description = "Best value yearly plan", Price = 5999, BillingCycle = BillingCycle.Yearly, Features = "[\"All Gold Features\",\"Unlimited PT\",\"Meal Planning\",\"Priority Booking\",\"Guest Passes\"]", IsActive = true, Color = "#8b5cf6", GymId = gym.Id },
        };
        db.MembershipPlans.AddRange(plans);

        // ── Sample Members ────────────────────────────────────────────────────
        var members = new List<Member>
        {
            new() { MemberId = "GF-0001", FirstName = "Arun", LastName = "Kumar", Email = "arun.kumar@example.com", Phone = "+91 9876543210", Gender = Gender.Male, DateOfBirth = new DateOnly(1992, 5, 14), Address = "42 MG Road", City = "Thiruvananthapuram", Status = MemberStatus.Active, MembershipPlanId = plans[1].Id, JoinDate = new DateOnly(2024, 1, 15), ExpiryDate = new DateOnly(2025, 1, 14), EmergencyContact = "Sunitha Kumar", EmergencyPhone = "+91 9876543299", GymId = gym.Id },
            new() { MemberId = "GF-0002", FirstName = "Priya", LastName = "Nair", Email = "priya.nair@example.com", Phone = "+91 9876543211", Gender = Gender.Female, DateOfBirth = new DateOnly(1995, 8, 22), Address = "15 Pattom", City = "Thiruvananthapuram", Status = MemberStatus.Active, MembershipPlanId = plans[2].Id, JoinDate = new DateOnly(2024, 2, 1), ExpiryDate = new DateOnly(2024, 5, 1), EmergencyContact = "Sajan Nair", EmergencyPhone = "+91 9876543298", GymId = gym.Id },
            new() { MemberId = "GF-0003", FirstName = "Rahul", LastName = "Menon", Email = "rahul.menon@example.com", Phone = "+91 9876543212", Gender = Gender.Male, DateOfBirth = new DateOnly(1988, 11, 30), Address = "8 Kowdiar", City = "Thiruvananthapuram", Status = MemberStatus.Expired, MembershipPlanId = plans[0].Id, JoinDate = new DateOnly(2023, 5, 1), ExpiryDate = new DateOnly(2024, 4, 30), EmergencyContact = "Lakshmi Menon", EmergencyPhone = "+91 9876543297", GymId = gym.Id },
            new() { MemberId = "GF-0004", FirstName = "Deepika", LastName = "Suresh", Email = "deepika.s@example.com", Phone = "+91 9876543213", Gender = Gender.Female, DateOfBirth = new DateOnly(1998, 3, 18), Address = "5 Vellayambalam", City = "Thiruvananthapuram", Status = MemberStatus.Active, MembershipPlanId = plans[0].Id, JoinDate = new DateOnly(2024, 4, 1), ExpiryDate = new DateOnly(2024, 6, 30), EmergencyContact = "Suresh S", EmergencyPhone = "+91 9876543296", GymId = gym.Id },
            new() { MemberId = "GF-0005", FirstName = "Vijay", LastName = "Kumar", Email = "vijay.kumar@example.com", Phone = "+91 9876543214", Gender = Gender.Male, DateOfBirth = new DateOnly(1990, 7, 7), Address = "22 Karamana", City = "Thiruvananthapuram", Status = MemberStatus.Active, MembershipPlanId = plans[3].Id, JoinDate = new DateOnly(2024, 1, 1), ExpiryDate = new DateOnly(2024, 12, 31), EmergencyContact = "Meena Kumar", EmergencyPhone = "+91 9876543295", GymId = gym.Id },
        };
        db.Members.AddRange(members);

        // ── Sample Transactions ───────────────────────────────────────────────
        var transactions = new List<Transaction>
        {
            new() { TransactionId = "TXN-001", MemberId = members[0].Id, Type = TransactionType.MembershipFee, Description = "Premium Plan - Monthly", Amount = 1999, PaymentMethod = PaymentMethod.UPI, Status = PaymentStatus.Paid, PaidAt = DateTime.UtcNow.AddDays(-2), GymId = gym.Id },
            new() { TransactionId = "TXN-002", MemberId = members[1].Id, Type = TransactionType.MembershipFee, Description = "Gold Plan - Q2 Renewal", Amount = 2999, PaymentMethod = PaymentMethod.Card, Status = PaymentStatus.Paid, PaidAt = DateTime.UtcNow.AddDays(-1), GymId = gym.Id },
            new() { TransactionId = "TXN-003", MemberId = members[2].Id, Type = TransactionType.MembershipFee, Description = "Basic Plan - Monthly", Amount = 999, PaymentMethod = PaymentMethod.Cash, Status = PaymentStatus.Pending, DueDate = DateTime.UtcNow.AddDays(3), GymId = gym.Id },
            new() { TransactionId = "TXN-004", MemberId = members[4].Id, Type = TransactionType.ProductSale, Description = "Whey Protein 1kg x2", Amount = 3600, PaymentMethod = PaymentMethod.Card, Status = PaymentStatus.Paid, PaidAt = DateTime.UtcNow.AddDays(-3), GymId = gym.Id },
            new() { TransactionId = "TXN-005", MemberId = members[0].Id, Type = TransactionType.PersonalTraining, Description = "PT Session Pack x5", Amount = 4500, PaymentMethod = PaymentMethod.UPI, Status = PaymentStatus.Paid, PaidAt = DateTime.UtcNow.AddDays(-5), GymId = gym.Id },
        };
        db.Transactions.AddRange(transactions);

        // ── Inventory Items ───────────────────────────────────────────────────
        var inventory = new List<InventoryItem>
        {
            new() { Sku = "EQ-DUMB-001", Name = "Rubber Dumbbell Set (5-50kg)", Category = "Equipment", Description = "Heavy-duty rubber coated dumbbells", Quantity = 8, MinQuantity = 5, Unit = "sets", PurchasePrice = 15000, SellingPrice = 0, Supplier = "FitGear India", Location = "Weights Area", LastRestocked = DateTime.UtcNow.AddMonths(-2), GymId = gym.Id },
            new() { Sku = "SUP-WHEY-001", Name = "Whey Protein 1kg", Category = "Supplements", Description = "Premium whey protein concentrate", Quantity = 3, MinQuantity = 10, Unit = "units", PurchasePrice = 1200, SellingPrice = 1800, Supplier = "MuscleBlaze", Location = "Store Room", LastRestocked = DateTime.UtcNow.AddMonths(-1), GymId = gym.Id },
            new() { Sku = "ACC-GLOVE-001", Name = "Workout Gloves", Category = "Accessories", Description = "Non-slip workout gloves", Quantity = 0, MinQuantity = 10, Unit = "pairs", PurchasePrice = 250, SellingPrice = 450, Supplier = "SportMax", Location = "Counter", GymId = gym.Id },
            new() { Sku = "CARE-TOW-001", Name = "Gym Towels", Category = "Facilities", Description = "Microfiber gym towels", Quantity = 45, MinQuantity = 20, Unit = "pieces", PurchasePrice = 150, SellingPrice = 250, Supplier = "CleanPro", Location = "Locker Room", LastRestocked = DateTime.UtcNow.AddDays(-15), GymId = gym.Id },
            new() { Sku = "SUP-BCAA-001", Name = "BCAA Powder 250g", Category = "Supplements", Description = "Branched-chain amino acids", Quantity = 5, MinQuantity = 8, Unit = "units", PurchasePrice = 800, SellingPrice = 1200, Supplier = "MuscleBlaze", Location = "Store Room", LastRestocked = DateTime.UtcNow.AddMonths(-2), GymId = gym.Id },
        };
        db.InventoryItems.AddRange(inventory);

        // ── Products ──────────────────────────────────────────────────────────
        var products = new List<Product>
        {
            new() { Name = "Whey Protein Isolate 2kg", Brand = "MuscleBlaze", Category = "Supplements", Description = "Ultra-pure whey protein isolate for lean muscle building.", Price = 3599, OriginalPrice = 4199, Stock = 24, Sku = "MB-WPI-2KG", IsActive = true, IsFeatured = true, Rating = 4.7m, ReviewCount = 234, GymId = gym.Id },
            new() { Name = "Pre-Workout Energy Blast", Brand = "Dymatize", Category = "Pre-Workout", Description = "Explosive energy and focus for intense training sessions.", Price = 1899, Stock = 12, Sku = "DY-PW-300G", IsActive = true, Rating = 4.3m, ReviewCount = 89, GymId = gym.Id },
            new() { Name = "Resistance Band Set", Brand = "Decathlon", Category = "Equipment", Description = "Set of 5 resistance bands for home and gym workouts.", Price = 999, Stock = 30, Sku = "DEC-RBS-001", IsActive = true, Rating = 4.5m, ReviewCount = 156, GymId = gym.Id },
            new() { Name = "Compression Shorts", Brand = "Reebok", Category = "Apparel", Description = "High-performance compression shorts for maximum support.", Price = 1499, OriginalPrice = 1999, Stock = 18, Sku = "RBK-CS-M", IsActive = true, IsFeatured = true, Rating = 4.2m, ReviewCount = 67, GymId = gym.Id },
        };
        db.Products.AddRange(products);

        // ── Promo Codes ───────────────────────────────────────────────────────
        var promos = new List<PromoCode>
        {
            new() { Code = "WELCOME50", Description = "Welcome offer for new members", DiscountType = DiscountType.Percentage, DiscountValue = 50, MinPurchase = 999, MaxUses = 100, UsedCount = 67, ValidFrom = new DateTime(2024, 1, 1), ValidTo = new DateTime(2024, 12, 31), ApplicableFor = PromoApplicableFor.NewMembers, GymId = gym.Id },
            new() { Code = "RENEW20", Description = "20% off on plan renewals", DiscountType = DiscountType.Percentage, DiscountValue = 20, MinPurchase = 0, MaxUses = 200, UsedCount = 134, ValidFrom = new DateTime(2024, 3, 1), ValidTo = new DateTime(2024, 5, 31), ApplicableFor = PromoApplicableFor.RenewalOnly, GymId = gym.Id },
            new() { Code = "FLAT500", Description = "Flat ₹500 off on premium plans", DiscountType = DiscountType.FixedAmount, DiscountValue = 500, MinPurchase = 1999, MaxUses = 50, UsedCount = 50, ValidFrom = new DateTime(2024, 2, 1), ValidTo = new DateTime(2024, 3, 31), ApplicableFor = PromoApplicableFor.All, GymId = gym.Id },
            new() { Code = "SUMMER30", Description = "Summer sale — 30% off everything", DiscountType = DiscountType.Percentage, DiscountValue = 30, MinPurchase = 0, MaxUses = 150, UsedCount = 0, ValidFrom = new DateTime(2025, 6, 1), ValidTo = new DateTime(2025, 8, 31), ApplicableFor = PromoApplicableFor.All, GymId = gym.Id },
        };
        db.PromoCodes.AddRange(promos);

        // ── Staff ─────────────────────────────────────────────────────────────
        var staff = new List<Staff>
        {
            new() { StaffId = "STF-001", FirstName = "Suresh", LastName = "P", Email = "suresh.p@gymflow.com", Phone = "+91 9876500001", Role = StaffRole.Trainer, Salary = 35000, JoinDate = new DateOnly(2023, 1, 1), IsActive = true, Specializations = "[\"Strength Training\",\"HIIT\",\"Nutrition\"]", GymId = gym.Id },
            new() { StaffId = "STF-002", FirstName = "Meera", LastName = "K", Email = "meera.k@gymflow.com", Phone = "+91 9876500002", Role = StaffRole.Receptionist, Salary = 22000, JoinDate = new DateOnly(2023, 3, 1), IsActive = true, GymId = gym.Id },
            new() { StaffId = "STF-003", FirstName = "Binesh", LastName = "M", Email = "binesh.m@gymflow.com", Phone = "+91 9876500003", Role = StaffRole.Manager, Salary = 55000, JoinDate = new DateOnly(2022, 6, 1), IsActive = true, GymId = gym.Id },
            new() { StaffId = "STF-004", FirstName = "Lakshmi", LastName = "S", Email = "lakshmi.s@gymflow.com", Phone = "+91 9876500004", Role = StaffRole.Trainer, Salary = 32000, JoinDate = new DateOnly(2023, 6, 1), IsActive = true, Specializations = "[\"Yoga\",\"Zumba\",\"Flexibility\"]", GymId = gym.Id },
        };
        db.Staff.AddRange(staff);

        await db.SaveChangesAsync();
        Console.WriteLine("✅ Database seeded successfully.");
    }
}
