using MedicalServices.Domain.Entities;

namespace MedicalServices.Web.Services;

public interface IFacilityCatalog
{
    Task<IReadOnlyList<Facility>> GetAllAsync(CancellationToken ct = default);
    Task<Facility?> FindAsync(string facilityId, CancellationToken ct = default);
}
