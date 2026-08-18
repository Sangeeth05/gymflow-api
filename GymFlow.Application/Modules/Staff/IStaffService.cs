namespace GymFlow.Application.Modules.Staff;

public interface IStaffService
{
    Task<List<object>> GetAllAsync(Guid gymId, string? role);
    Task<object?> GetByIdAsync(Guid id, Guid gymId);
    Task<object> CreateAsync(Guid gymId, object dto);
    Task<object?> UpdateAsync(Guid id, Guid gymId, object dto);
    Task<bool> DeleteAsync(Guid id, Guid gymId);
}
