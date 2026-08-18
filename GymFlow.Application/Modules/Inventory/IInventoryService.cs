using GymFlow.Application.Common;

namespace GymFlow.Application.Modules.Inventory;

public interface IInventoryService
{
    Task<PaginatedResponse<object>> GetAllAsync(Guid gymId, int page, int pageSize, string? search, string? category);
    Task<object?> GetByIdAsync(Guid id, Guid gymId);
    Task<object> CreateAsync(Guid gymId, object dto);
    Task<object?> UpdateAsync(Guid id, Guid gymId, object dto);
    Task<bool> DeleteAsync(Guid id, Guid gymId);
}
