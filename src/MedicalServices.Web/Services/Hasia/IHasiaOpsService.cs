using MedicalServices.Domain.Entities;

namespace MedicalServices.Web.Services.Hasia;

public interface IHasiaOpsService
{
    Task EnsureSeededAsync(CancellationToken ct = default);

    Task<IReadOnlyList<PrisonInmate>> ListInmatesAsync(CancellationToken ct = default);
    Task<PrisonInmate?> FindInmateAsync(string inmateNo, CancellationToken ct = default);
    Task<PrisonInmate> CreateInmateAsync(CreateInmateRequest request, CancellationToken ct = default);

    Task<IReadOnlyList<PrisonCase>> ListCasesAsync(string? status = null, bool archiveOnly = false, CancellationToken ct = default);
    Task<PrisonCase?> GetCaseAsync(Guid id, CancellationToken ct = default);
    Task<PrisonCase> OpenTriageCaseAsync(OpenTriageRequest request, CancellationToken ct = default);
    Task<PrisonCase> CompleteStepAsync(Guid caseId, CaseStepRequest request, CancellationToken ct = default);

    Task<IReadOnlyList<PrisonPharmacyItem>> ListStockAsync(string pharmacy, CancellationToken ct = default);
    Task<IReadOnlyList<PrisonPrescription>> ListPrescriptionsAsync(string pharmacy, bool pendingOnly = true, CancellationToken ct = default);
    Task<IReadOnlyList<PrisonDispense>> ListDispensesAsync(string pharmacy, CancellationToken ct = default);
    Task PrescribeAsync(PrescribeRequest request, CancellationToken ct = default);
    Task DispenseAsync(DispenseRequest request, CancellationToken ct = default);

    Task<Dictionary<string, int>> LoadByServiceAsync(CancellationToken ct = default);
}

public sealed record StayDto(string Place, string Entry, string? Exit);

public sealed record CreateInmateRequest(
    string InmateNo,
    string FileNo,
    string Nationality,
    string? BirthPlace,
    string? BirthDate,
    string BloodType,
    string Sex,
    string Marital,
    string Place,
    string EntryDate,
    string ChronicCsv,
    string Allergies,
    string? RecordJson,
    IReadOnlyList<StayDto> Stays);

public sealed record OpenTriageRequest(
    string Triager,
    string InmateNo,
    string Complaint,
    string ToServiceId);

public sealed record CaseMedLine(string Drug, int Qty);

public sealed record CaseStepRequest(
    string Provider,
    string? Diagnosis,
    string? Procedures,
    IReadOnlyList<CaseMedLine> Meds,
    string Action,
    string? TargetServiceId,
    string? HoldReason);

public sealed record PrescribeRequest(
    string Pharmacy,
    string InmateNo,
    string DrugName,
    int Quantity,
    string Doctor,
    string? CaseCode);

public sealed record DispenseRequest(
    string Pharmacy,
    string InmateNo,
    string DrugName,
    int Quantity,
    string? CaseCode,
    string? By,
    Guid? PrescriptionId);
