namespace GymFlow.Application.Modules.Promotions;

public record ValidatePromoDto(string Code, decimal Amount);

public interface IPromoCodeService
{
    Task<List<object>> GetAllAsync(Guid gymId);
    Task<object> CreateAsync(Guid gymId, object dto);
    Task<object?> UpdateAsync(Guid id, Guid gymId, object dto);
    Task<bool> DeleteAsync(Guid id, Guid gymId);
    Task<object> ValidateAsync(string code, Guid gymId, decimal amount);
}
