using GymFlow.Application.Common;

namespace GymFlow.Application.Modules.Finance;

public interface IFinanceService
{
    Task<PaginatedResponse<object>> GetTransactionsAsync(Guid gymId, int page, int pageSize, string? search, string? status);
    Task<object> GetSummaryAsync(Guid gymId);
    Task<object> CreateTransactionAsync(Guid gymId, object dto);
}
