namespace MedicalServices.Domain.Enums;

public enum PatientCaseStatus
{
    AtReception = 0,
    InClinic = 1,
    ReturnedToReception = 2,
    Closed = 3,
    InDepartment = 4
}

public static class PatientCaseStatusLabels
{
    public static string ToArabic(PatientCaseStatus status) => status switch
    {
        PatientCaseStatus.AtReception => "في الاستقبال",
        PatientCaseStatus.InClinic => "في العيادة",
        PatientCaseStatus.ReturnedToReception => "مُعاد للاستقبال",
        PatientCaseStatus.Closed => "مغلق",
        PatientCaseStatus.InDepartment => "قيد المعالجة",
        _ => status.ToString()
    };
}
