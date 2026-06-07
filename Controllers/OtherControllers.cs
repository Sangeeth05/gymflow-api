using GymFlow.API.Services;
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
    public async Task<IActionResult> GetStats() => Ok(await _svc.GetStatsAsync(GetGymId()));

    [HttpGet("activity")]
    public async Task<IActionResult> GetActivity() => Ok(await _svc.GetRecentActivityAsync(GetGymId()));

    [HttpGet("revenue-chart")]
    public async Task<IActionResult> GetRevenueChart() => Ok(await _svc.GetRevenueChartAsync(GetGymId()));

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary() => Ok(await _svc.GetSummaryAsync(GetGymId()));

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
    public async Task<IActionResult> GetSummary()
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
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null, [FromQuery] string? category = null)
        => Ok(await _svc.GetAllAsync(GetGymId(), page, pageSize, search, category));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var item = await _svc.GetByIdAsync(id, GetGymId());
        return item == null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] object dto)
        => Ok(await _svc.CreateAsync(GetGymId(), dto));

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] object dto)
    {
        var result = await _svc.UpdateAsync(id, GetGymId(), dto);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
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
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null, [FromQuery] string? category = null)
        => Ok(await _svc.GetAllAsync(GetGymId(), page, pageSize, search, category));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] object dto)
        => Ok(await _svc.CreateAsync(GetGymId(), dto));

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] object dto)
    {
        var result = await _svc.UpdateAsync(id, GetGymId(), dto);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
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
    public async Task<IActionResult> GetAll() => Ok(await _svc.GetAllAsync(GetGymId()));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] object dto)
        => Ok(await _svc.CreateAsync(GetGymId(), dto));

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] object dto)
    {
        var result = await _svc.UpdateAsync(id, GetGymId(), dto);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
        => await _svc.DeleteAsync(id, GetGymId()) ? NoContent() : NotFound();

    [HttpPost("validate")]
    [AllowAnonymous]
    public async Task<IActionResult> Validate([FromBody] ValidatePromoDto dto)
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
    public async Task<IActionResult> GetAll([FromQuery] string? role = null)
        => Ok(await _svc.GetAllAsync(GetGymId(), role));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var s = await _svc.GetByIdAsync(id, GetGymId());
        return s == null ? NotFound() : Ok(s);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] object dto)
        => Ok(await _svc.CreateAsync(GetGymId(), dto));

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] object dto)
    {
        var result = await _svc.UpdateAsync(id, GetGymId(), dto);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
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
    public async Task<IActionResult> GetAll()
        => Ok(await _svc.GetAllPlansAsync(GetGymId()));

    private Guid GetGymId() =>
        Guid.TryParse(User.FindFirst("gymId")?.Value, out var id) ? id : Guid.Empty;
}

public record ValidatePromoDto(string Code, decimal Amount);
