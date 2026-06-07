using GymFlow.API.DTOs;

namespace GymFlow.API.Services;

public interface IAuthService
{
    Task<AuthResponse?> LoginAsync(LoginRequest request);
    Task<AuthResponse?> RefreshTokenAsync(string refreshToken);
    Task LogoutAsync(string refreshToken);
    Task<UserDto?> GetUserByIdAsync(Guid id);
}

public interface IMemberService
{
    Task<PaginatedResponse<MemberDto>> GetAllAsync(Guid gymId, int page, int pageSize,
        string? search, string? status, string? sortBy, string? sortOrder);
    Task<MemberDto?> GetByIdAsync(Guid id, Guid gymId);
    Task<MemberDto> CreateAsync(CreateMemberDto dto, Guid gymId);
    Task<MemberDto?> UpdateAsync(Guid id, UpdateMemberDto dto, Guid gymId);
    Task<bool> DeleteAsync(Guid id, Guid gymId);
    Task CheckInAsync(Guid memberId, Guid gymId);
    Task<List<MemberDto>> GetExpiringAsync(Guid gymId, int days);
    Task<List<object>> GetAllPlansAsync(Guid gymId);
}

public interface IFinanceService
{
    Task<PaginatedResponse<object>> GetTransactionsAsync(Guid gymId, int page, int pageSize, string? search, string? status);
    Task<object> GetSummaryAsync(Guid gymId);
    Task<object> CreateTransactionAsync(Guid gymId, object dto);
}

public interface IInventoryService
{
    Task<PaginatedResponse<object>> GetAllAsync(Guid gymId, int page, int pageSize, string? search, string? category);
    Task<object?> GetByIdAsync(Guid id, Guid gymId);
    Task<object> CreateAsync(Guid gymId, object dto);
    Task<object?> UpdateAsync(Guid id, Guid gymId, object dto);
    Task<bool> DeleteAsync(Guid id, Guid gymId);
}

public interface IProductService
{
    Task<PaginatedResponse<object>> GetAllAsync(Guid gymId, int page, int pageSize, string? search, string? category);
    Task<object> CreateAsync(Guid gymId, object dto);
    Task<object?> UpdateAsync(Guid id, Guid gymId, object dto);
    Task<bool> DeleteAsync(Guid id, Guid gymId);
}

public interface IPromoCodeService
{
    Task<List<object>> GetAllAsync(Guid gymId);
    Task<object> CreateAsync(Guid gymId, object dto);
    Task<object?> UpdateAsync(Guid id, Guid gymId, object dto);
    Task<bool> DeleteAsync(Guid id, Guid gymId);
    Task<object> ValidateAsync(string code, Guid gymId, decimal amount);
}

public interface IStaffService
{
    Task<List<object>> GetAllAsync(Guid gymId, string? role);
    Task<object?> GetByIdAsync(Guid id, Guid gymId);
    Task<object> CreateAsync(Guid gymId, object dto);
    Task<object?> UpdateAsync(Guid id, Guid gymId, object dto);
    Task<bool> DeleteAsync(Guid id, Guid gymId);
}

public interface IDashboardService
{
    Task<DashboardStatsDto> GetStatsAsync(Guid gymId);
    Task<List<RecentActivityDto>> GetRecentActivityAsync(Guid gymId);
    Task<List<RevenueChartItem>> GetRevenueChartAsync(Guid gymId);
    Task<object> GetSummaryAsync(Guid gymId);
}
