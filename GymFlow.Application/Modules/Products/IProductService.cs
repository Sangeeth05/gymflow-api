using GymFlow.Application.Common;

namespace GymFlow.Application.Modules.Products;

public interface IProductService
{
    Task<PaginatedResponse<object>> GetAllAsync(Guid gymId, int page, int pageSize, string? search, string? category);
    Task<object> CreateAsync(Guid gymId, object dto);
    Task<object?> UpdateAsync(Guid id, Guid gymId, object dto);
    Task<bool> DeleteAsync(Guid id, Guid gymId);
}
