namespace GymFlow.Application.Modules.Gyms;

public record GymApprovalDto(
    Guid Id,
    string Name,
    string Email,
    string City,
    string Country,
    string Status,
    DateTime CreatedAt,
    string OwnerName,
    string OwnerEmail
);
