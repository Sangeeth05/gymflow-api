using System.ComponentModel.DataAnnotations;

namespace GymFlow.Application.Modules.Identity;

public record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required, MinLength(6)] string Password
);

public record RefreshTokenRequest([Required] string RefreshToken);

public record AuthResponse(
    string Token,
    string RefreshToken,
    UserDto User
);

public record UserDto(
    Guid Id,
    string Name,
    string Email,
    string Role,
    Guid? GymId,
    string? GymName,
    string? GymStatus
);

// ── Gym Onboarding / Signup ─────────────────────────────────────────────────
public record GymSignupRequest(
    // Gym details
    [Required, MaxLength(200)] string GymName,
    [Required, MaxLength(500)] string Address,
    [Required, MaxLength(100)] string City,
    [Required, MaxLength(100)] string Country,
    [Required, MaxLength(20)]  string Phone,
    [Required, EmailAddress]   string GymEmail,

    // First GymAdmin (owner) account
    [Required, MaxLength(200)] string AdminName,
    [Required, EmailAddress]   string AdminEmail,
    [Required, MinLength(6)]   string Password
);

public record GymSignupResponse(
    Guid GymId,
    string Message
);
