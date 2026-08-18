using GymFlow.Application.Modules.Gyms;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymFlow.API.Controllers;

// SuperAdmin-only: review and approve/suspend gyms that signed up via onboarding.
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "SuperAdmin")]
public class GymsController : ControllerBase
{
    private readonly IGymService _gymService;
    public GymsController(IGymService gymService) => _gymService = gymService;

    /// <summary>List gyms, optionally filtered by status (e.g. ?status=PendingApproval)</summary>
    [HttpGet]
    public async Task<IActionResult> GetGyms([FromQuery] string? status)
        => Ok(await _gymService.GetGymsAsync(status));

    /// <summary>Activate a pending (or suspended) gym so its GymAdmin can log in</summary>
    [HttpPatch("{id}/activate")]
    public async Task<IActionResult> Activate(Guid id)
        => await _gymService.ActivateAsync(id)
            ? Ok(new { message = "Gym activated." })
            : NotFound();

    /// <summary>Suspend an active gym, blocking further logins until reactivated</summary>
    [HttpPatch("{id}/suspend")]
    public async Task<IActionResult> Suspend(Guid id)
        => await _gymService.SuspendAsync(id)
            ? Ok(new { message = "Gym suspended." })
            : NotFound();
}
