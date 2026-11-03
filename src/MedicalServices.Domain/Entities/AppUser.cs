using MedicalServices.Domain.Enums;

namespace MedicalServices.Domain.Entities;

public sealed class AppUser
{
    public required string Username { get; init; }
    public required string DisplayNameAr { get; init; }
    public required string Password { get; init; }
    public AppRole Role { get; init; }

    /// <summary>Linked facility for facility users; null for directors/admins.</summary>
    public string? FacilityId { get; init; }
}
