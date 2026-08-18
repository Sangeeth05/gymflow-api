using GymFlow.Application.Modules.Dashboard;
using GymFlow.Application.Modules.Finance;
using GymFlow.Application.Modules.Inventory;
using GymFlow.Application.Modules.Members;
using GymFlow.Application.Modules.Products;
using GymFlow.Application.Modules.Promotions;
using GymFlow.Application.Modules.Staff;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymFlow.API.Controllers;

// ── Dashboard ─────────────────────────────────────────────────────────────────
[ApiController, Route("api/[controller]"), Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _svc;
    public DashboardController(IDashboardService svc) => _svc = svc;

    [HttpGet("stats")]
    public async Task<IActionResult> GetDashboardStats() => Ok(await _svc.GetStatsAsync(GetGymId()));

    [HttpGet("activity")]
    public async Task<IActionResult> GetDashboardActivity() => Ok(await _svc.GetRecentActivityAsync(GetGymId()));

    [HttpGet("revenue-chart")]
    public async Task<IActionResult> GetDashboardRevenueChart() => Ok(await _svc.GetRevenueChartAsync(GetGymId()));

    [HttpGet("summary")]
    public async Task<IActionResult> GetDashboardSummary() => Ok(await _svc.GetSummaryAsync(GetGymId()));

    private Guid GetGymId() =>
        Guid.TryParse(User.FindFirst("gymId")?.Value, out var id) ? id : Guid.Empty;
}

// ── Finance ───────────────────────────────────────────────────────────────────
[ApiController, Route("api/[controller]"), Authorize]
public class FinanceController : ControllerBase
{
    private readonly IFinanceService _svc;
    public FinanceController(IFinanceService svc) => _svc = svc;

    [HttpGet("transactions")]
    public async Task<IActionResult> GetTransactions(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null, [FromQuery] string? status = null)
        => Ok(await _svc.GetTransactionsAsync(GetGymId(), page, pageSize, search, status));

    [HttpGet("summary")]
    public async Task<IActionResult> GetFinanceSummary()
        => Ok(await _svc.GetSummaryAsync(GetGymId()));

    [HttpPost("transactions")]
    public async Task<IActionResult> CreateTransaction([FromBody] object dto)
        => Ok(await _svc.CreateTransactionAsync(GetGymId(), dto));

    private Guid GetGymId() =>
        Guid.TryParse(User.FindFirst("gymId")?.Value, out var id) ? id : Guid.Empty;
}

// ── Inventory ─────────────────────────────────────────────────────────────────
[ApiController, Route("api/[controller]"), Authorize]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _svc;
    public InventoryController(IInventoryService svc) => _svc = svc;

    [HttpGet]
    public async Task<IActionResult> GetAllInventory(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null, [FromQuery] string? category = null)
        => Ok(await _svc.GetAllAsync(GetGymId(), page, pageSize, search, category));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetInventoryById(Guid id)
    {
        var item = await _svc.GetByIdAsync(id, GetGymId());
        return item == null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> CreateInventoryItem([FromBody] object dto)
        => Ok(await _svc.CreateAsync(GetGymId(), dto));

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateInventoryItem(Guid id, [FromBody] object dto)
    {
        var result = await _svc.UpdateAsync(id, GetGymId(), dto);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteInventoryItem(Guid id)
    {
        var ok = await _svc.DeleteAsync(id, GetGymId());
        return ok ? NoContent() : NotFound();
    }

    private Guid GetGymId() =>
        Guid.TryParse(User.FindFirst("gymId")?.Value, out var id) ? id : Guid.Empty;
}

// ── Products ──────────────────────────────────────────────────────────────────
[ApiController, Route("api/[controller]"), Authorize]
public class ProductsController : ControllerBase
{
    private readonly IProductService _svc;
    public ProductsController(IProductService svc) => _svc = svc;

    [HttpGet]
    public async Task<IActionResult> GetAllProducts(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null, [FromQuery] string? category = null)
        => Ok(await _svc.GetAllAsync(GetGymId(), page, pageSize, search, category));

    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] object dto)
        => Ok(await _svc.CreateAsync(GetGymId(), dto));

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] object dto)
    {
        var result = await _svc.UpdateAsync(id, GetGymId(), dto);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(Guid id)
        => await _svc.DeleteAsync(id, GetGymId()) ? NoContent() : NotFound();

    private Guid GetGymId() =>
        Guid.TryParse(User.FindFirst("gymId")?.Value, out var id) ? id : Guid.Empty;
}

// ── Promo Codes ───────────────────────────────────────────────────────────────
[ApiController, Route("api/promo-codes"), Authorize]
public class PromoCodesController : ControllerBase
{
    private readonly IPromoCodeService _svc;
    public PromoCodesController(IPromoCodeService svc) => _svc = svc;

    [HttpGet]
    public async Task<IActionResult> GetAllPromoCodes() => Ok(await _svc.GetAllAsync(GetGymId()));

    [HttpPost]
    public async Task<IActionResult> CreatePromoCode([FromBody] object dto)
        => Ok(await _svc.CreateAsync(GetGymId(), dto));

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePromoCode(Guid id, [FromBody] object dto)
    {
        var result = await _svc.UpdateAsync(id, GetGymId(), dto);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePromoCode(Guid id)
        => await _svc.DeleteAsync(id, GetGymId()) ? NoContent() : NotFound();

    [HttpPost("validate")]
    [AllowAnonymous]
    public async Task<IActionResult> ValidatePromoCode([FromBody] ValidatePromoDto dto)
        => Ok(await _svc.ValidateAsync(dto.Code, GetGymId(), dto.Amount));

    private Guid GetGymId() =>
        Guid.TryParse(User.FindFirst("gymId")?.Value, out var id) ? id : Guid.Empty;
}

// ── Staff ─────────────────────────────────────────────────────────────────────
[ApiController, Route("api/[controller]"), Authorize]
public class StaffController : ControllerBase
{
    private readonly IStaffService _svc;
    public StaffController(IStaffService svc) => _svc = svc;

    [HttpGet]
    public async Task<IActionResult> GetAllStaff([FromQuery] string? role = null)
        => Ok(await _svc.GetAllAsync(GetGymId(), role));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetStaffById(Guid id)
    {
        var s = await _svc.GetByIdAsync(id, GetGymId());
        return s == null ? NotFound() : Ok(s);
    }

    [HttpPost]
    public async Task<IActionResult> CreateStaff([FromBody] object dto)
        => Ok(await _svc.CreateAsync(GetGymId(), dto));

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateStaff(Guid id, [FromBody] object dto)
    {
        var result = await _svc.UpdateAsync(id, GetGymId(), dto);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteStaff(Guid id)
        => await _svc.DeleteAsync(id, GetGymId()) ? NoContent() : NotFound();

    private Guid GetGymId() =>
        Guid.TryParse(User.FindFirst("gymId")?.Value, out var id) ? id : Guid.Empty;
}

// ── Membership Plans ──────────────────────────────────────────────────────────
[ApiController, Route("api/membership-plans"), Authorize]
public class MembershipPlansController : ControllerBase
{
    private readonly IMemberService _svc;
    public MembershipPlansController(IMemberService svc) => _svc = svc;

    [HttpGet]
    public async Task<IActionResult> GetAllMembershipPlans()
        => Ok(await _svc.GetAllPlansAsync(GetGymId()));

    private Guid GetGymId() =>
        Guid.TryParse(User.FindFirst("gymId")?.Value, out var id) ? id : Guid.Empty;
}
