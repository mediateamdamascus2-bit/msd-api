namespace MedicalServices.Domain.Entities;

public class PrisonInmate
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FacilityId { get; set; } = string.Empty;
    public string InmateNo { get; set; } = string.Empty;
    public string FileNo { get; set; } = string.Empty;
    public string Nationality { get; set; } = "سوري";
    public string? BirthPlace { get; set; }
    public string? BirthDate { get; set; }
    public string BloodType { get; set; } = "—";
    public string Sex { get; set; } = "ذكر";
    public string Marital { get; set; } = "عازب";
    public string Place { get; set; } = "حسيا";
    public string EntryDate { get; set; } = string.Empty;
    public string ChronicCsv { get; set; } = string.Empty;
    public string Allergies { get; set; } = "—";
    /// <summary>JSON: intake form fields, history, signs, vitals, exam, marks.</summary>
    public string? RecordJson { get; set; }
    /// <summary>JSON array of diagnoses {t,d,by}.</summary>
    public string DiagnosesJson { get; set; } = "[]";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public List<PrisonStay> Stays { get; set; } = [];
}

public class PrisonStay
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid InmateId { get; set; }
    public PrisonInmate? Inmate { get; set; }
    public int SortOrder { get; set; }
    public string Place { get; set; } = string.Empty;
    public string Entry { get; set; } = string.Empty;
    public string? Exit { get; set; }
}

public class PrisonCase
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FacilityId { get; set; } = string.Empty;
    public string CaseCode { get; set; } = string.Empty;
    public string InmateNo { get; set; } = string.Empty;
    public string Triager { get; set; } = string.Empty;
    public string Complaint { get; set; } = string.Empty;
    public string AtServiceId { get; set; } = "p-er";
    public string Status { get; set; } = "جارية";
    public string? HoldReason { get; set; }
    public string OpenedTime { get; set; } = string.Empty;
    public DateOnly Day { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public List<PrisonCaseStep> Steps { get; set; } = [];
}

public class PrisonCaseStep
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CaseId { get; set; }
    public PrisonCase? Case { get; set; }
    public int SortOrder { get; set; }
    public string Time { get; set; } = string.Empty;
    public string AtServiceId { get; set; } = string.Empty;
    public string By { get; set; } = string.Empty;
    public string? Diagnosis { get; set; }
    public string? Procedures { get; set; }
    public string? Meds { get; set; }
    public string? Note { get; set; }
}

public class PrisonPharmacyItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FacilityId { get; set; } = string.Empty;
    public string Pharmacy { get; set; } = "internal";
    public string DrugName { get; set; } = string.Empty;
    public int Quantity { get; set; }
}

public class PrisonPrescription
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FacilityId { get; set; } = string.Empty;
    public string Pharmacy { get; set; } = "internal";
    public string InmateNo { get; set; } = string.Empty;
    public string DrugName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string Doctor { get; set; } = string.Empty;
    public string? CaseCode { get; set; }
    public string Status { get; set; } = "pending";
    public string CreatedTime { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public class PrisonDispense
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FacilityId { get; set; } = string.Empty;
    public string Pharmacy { get; set; } = "internal";
    public string InmateNo { get; set; } = string.Empty;
    public string DrugName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string? CaseCode { get; set; }
    public string? By { get; set; }
    public string DispensedTime { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
