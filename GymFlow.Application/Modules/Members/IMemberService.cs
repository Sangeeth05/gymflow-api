using GymFlow.Application.Common;

namespace GymFlow.Application.Modules.Members;

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
