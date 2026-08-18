namespace GymFlow.Application.Modules.Gyms;

public interface IGymService
{
    Task<List<GymApprovalDto>> GetGymsAsync(string? status);
    Task<bool> ActivateAsync(Guid gymId);
    Task<bool> SuspendAsync(Guid gymId);
}
