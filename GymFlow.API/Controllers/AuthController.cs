using GymFlow.Application.Modules.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>Login with email and password</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);

        if (result.Success)
            return Ok(result.Data);

        if (result.IsPendingApproval)
            return StatusCode(403, new { message = result.ErrorMessage, reason = "pending_approval" });

        if (result.IsSuspended)
            return StatusCode(403, new { message = result.ErrorMessage, reason = "suspended" });

        return Unauthorized(new { message = result.ErrorMessage });
    }

    /// <summary>Register a new gym + its first GymAdmin account (onboarding).
    /// The gym starts PendingApproval and cannot log in until a SuperAdmin activates it.</summary>
    [HttpPost("register-gym")]
    [AllowAnonymous]
    public async Task<IActionResult> RegisterGym([FromBody] GymSignupRequest request)
    {
        var result = await _authService.RegisterGymAsync(request);
        if (!result.Success)
            return Conflict(new { message = result.ErrorMessage });

        return Ok(new GymSignupResponse(
            result.GymId!.Value,
            "Your gym has been registered and is pending approval. We'll notify you once it's activated."));
    }

    /// <summary>Refresh access token</summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        var result = await _authService.RefreshTokenAsync(request.RefreshToken);
        if (result == null) return Unauthorized(new { message = "Invalid or expired refresh token" });
        return Ok(result);
    }

    /// <summary>Logout and revoke refresh token</summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
    {
        await _authService.LogoutAsync(request.RefreshToken);
        return Ok(new { message = "Logged out successfully" });
    }

    /// <summary>Get current user profile</summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me()
    {
        var userId = User.FindFirst("sub")?.Value;
        if (userId == null) return Unauthorized();
        var user = await _authService.GetUserByIdAsync(Guid.Parse(userId));
        if (user == null) return NotFound();
        return Ok(user);
    }
}
