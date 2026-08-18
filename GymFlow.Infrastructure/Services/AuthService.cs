using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using GymFlow.Application.Modules.Identity;
using GymFlow.Domain.Entities;
using GymFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace GymFlow.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public AuthService(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    public async Task<LoginResult> LoginAsync(LoginRequest request)
    {
        var user = await _db.Users
            .Include(u => u.Gym)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower() && !u.IsDeleted);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return LoginResult.Invalid("Invalid email or password");

        // SuperAdmin has no Gym and skips gym-status checks entirely.
        if (user.Role != UserRole.SuperAdmin)
        {
            if (user.Gym == null)
                return LoginResult.Invalid("This account is not linked to a gym");

            switch (user.Gym.Status)
            {
                case GymStatus.PendingApproval:
                    return LoginResult.Pending(
                        "Your gym is pending approval. We'll notify you once it's activated.");
                case GymStatus.Suspended:
                    return LoginResult.SuspendedResult(
                        "This gym account has been suspended. Please contact support.");
            }
        }

        var tokens = await GenerateTokensAsync(user);
        return LoginResult.Ok(tokens);
    }

    public async Task<RegisterGymResult> RegisterGymAsync(GymSignupRequest request)
    {
        var emailTaken = await _db.Users.AnyAsync(u => u.Email.ToLower() == request.AdminEmail.ToLower());
        if (emailTaken)
            return RegisterGymResult.Failure("An account with this email already exists.");

        var gymEmailTaken = await _db.Gyms.AnyAsync(g => g.Email.ToLower() == request.GymEmail.ToLower());
        if (gymEmailTaken)
            return RegisterGymResult.Failure("A gym with this email is already registered.");

        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            var gym = new Gym
            {
                Name = request.GymName,
                Address = request.Address,
                City = request.City,
                Country = request.Country,
                Phone = request.Phone,
                Email = request.GymEmail,
                Status = GymStatus.PendingApproval,
            };
            _db.Gyms.Add(gym);
            await _db.SaveChangesAsync();

            var owner = new User
            {
                Name = request.AdminName,
                Email = request.AdminEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = UserRole.GymAdmin,
                GymId = gym.Id,
            };
            _db.Users.Add(owner);
            await _db.SaveChangesAsync();

            await transaction.CommitAsync();
            return RegisterGymResult.Ok(gym.Id);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<AuthResponse?> RefreshTokenAsync(string refreshToken)
    {
        var token = await _db.RefreshTokens
            .Include(t => t.User).ThenInclude(u => u.Gym)
            .FirstOrDefaultAsync(t => t.Token == refreshToken && !t.IsRevoked && t.ExpiresAt > DateTime.UtcNow);

        if (token == null) return null;

        // A gym suspended after the token was issued shouldn't silently refresh back in.
        if (token.User.Role != UserRole.SuperAdmin && token.User.Gym?.Status != GymStatus.Active)
            return null;

        token.IsRevoked = true;
        await _db.SaveChangesAsync();

        return await GenerateTokensAsync(token.User);
    }

    public async Task LogoutAsync(string refreshToken)
    {
        var token = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.Token == refreshToken);
        if (token != null)
        {
            token.IsRevoked = true;
            await _db.SaveChangesAsync();
        }
    }

    public async Task<UserDto?> GetUserByIdAsync(Guid id)
    {
        var user = await _db.Users.Include(u => u.Gym)
            .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
        return user == null ? null : MapToDto(user);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────
    private async Task<AuthResponse> GenerateTokensAsync(User user)
    {
        var jwtSettings = _config.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"]!;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("role", user.Role.ToString()),
            new(ClaimTypes.Role, user.Role.ToString()),
        };

        // gymId omitted entirely for SuperAdmin (not scoped to a single gym)
        if (user.GymId.HasValue)
            claims.Add(new Claim("gymId", user.GymId.Value.ToString()));

        // linkedId lets Trainer/Member endpoints resolve "my profile" without a lookup
        if (user.StaffId.HasValue)
            claims.Add(new Claim("linkedId", user.StaffId.Value.ToString()));
        else if (user.MemberId.HasValue)
            claims.Add(new Claim("linkedId", user.MemberId.Value.ToString()));

        var expiry = DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["ExpiryMinutes"] ?? "60"));
        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiry,
            signingCredentials: creds
        );

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        var refreshBytes = RandomNumberGenerator.GetBytes(64);
        var refreshToken = Convert.ToBase64String(refreshBytes);

        var refreshTokenEntity = new RefreshToken
        {
            Token = refreshToken,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(int.Parse(jwtSettings["RefreshTokenExpiryDays"] ?? "7")),
        };
        _db.RefreshTokens.Add(refreshTokenEntity);
        await _db.SaveChangesAsync();

        return new AuthResponse(accessToken, refreshToken, MapToDto(user));
    }

    private static UserDto MapToDto(User user) => new(
        user.Id, user.Name, user.Email, user.Role.ToString(),
        user.GymId, user.Gym?.Name, user.Gym?.Status.ToString());
}
