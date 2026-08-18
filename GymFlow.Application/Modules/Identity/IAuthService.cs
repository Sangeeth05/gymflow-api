namespace GymFlow.Application.Modules.Identity;

// Wraps a login attempt so the controller can distinguish
// "wrong password" from "gym pending approval" from "gym suspended".
public class LoginResult
{
    public bool Success { get; init; }
    public AuthResponse? Data { get; init; }
    public string? ErrorMessage { get; init; }
    public bool IsPendingApproval { get; init; }
    public bool IsSuspended { get; init; }

    public static LoginResult Ok(AuthResponse data) => new() { Success = true, Data = data };
    public static LoginResult Invalid(string message) => new() { Success = false, ErrorMessage = message };
    public static LoginResult Pending(string message) => new() { Success = false, ErrorMessage = message, IsPendingApproval = true };
    public static LoginResult SuspendedResult(string message) => new() { Success = false, ErrorMessage = message, IsSuspended = true };
}

public class RegisterGymResult
{
    public bool Success { get; init; }
    public Guid? GymId { get; init; }
    public string? ErrorMessage { get; init; }

    public static RegisterGymResult Ok(Guid gymId) => new() { Success = true, GymId = gymId };
    public static RegisterGymResult Failure(string message) => new() { Success = false, ErrorMessage = message };
}

public interface IAuthService
{
    Task<LoginResult> LoginAsync(LoginRequest request);
    Task<RegisterGymResult> RegisterGymAsync(GymSignupRequest request);
    Task<AuthResponse?> RefreshTokenAsync(string refreshToken);
    Task LogoutAsync(string refreshToken);
    Task<UserDto?> GetUserByIdAsync(Guid id);
}
