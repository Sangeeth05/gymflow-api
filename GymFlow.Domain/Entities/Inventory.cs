using System.ComponentModel.DataAnnotations;
using GymFlow.Domain.Common;

namespace GymFlow.Domain.Entities;

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
