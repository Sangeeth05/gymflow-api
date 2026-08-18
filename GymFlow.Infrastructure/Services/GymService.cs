using GymFlow.Application.Modules.Gyms;
using GymFlow.Domain.Entities;
using GymFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GymFlow.Infrastructure.Services;

public class GymService : IGymService
{
    private readonly AppDbContext _db;
    public GymService(AppDbContext db) => _db = db;

    public async Task<List<GymApprovalDto>> GetGymsAsync(string? status)
    {
        var query = _db.Gyms.Where(g => !g.IsDeleted).AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<GymStatus>(status, true, out var parsedStatus))
            query = query.Where(g => g.Status == parsedStatus);

        return await query
            .OrderByDescending(g => g.CreatedAt)
            .Select(g => new GymApprovalDto(
                g.Id, g.Name, g.Email, g.City, g.Country, g.Status.ToString(), g.CreatedAt,
                g.Users.Where(u => u.Role == UserRole.GymAdmin).Select(u => u.Name).FirstOrDefault() ?? "",
                g.Users.Where(u => u.Role == UserRole.GymAdmin).Select(u => u.Email).FirstOrDefault() ?? ""
            ))
            .ToListAsync();
    }

    public async Task<bool> ActivateAsync(Guid gymId)
    {
        var gym = await _db.Gyms.FirstOrDefaultAsync(g => g.Id == gymId && !g.IsDeleted);
        if (gym == null) return false;
        gym.Status = GymStatus.Active;
        gym.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SuspendAsync(Guid gymId)
    {
        var gym = await _db.Gyms.FirstOrDefaultAsync(g => g.Id == gymId && !g.IsDeleted);
        if (gym == null) return false;
        gym.Status = GymStatus.Suspended;
        gym.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }
}
