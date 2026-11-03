using MedicalServices.Domain.Entities;
using MedicalServices.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MedicalServices.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Facility> Facilities => Set<Facility>();
    public DbSet<DailyAdmission> DailyAdmissions => Set<DailyAdmission>();
    public DbSet<PatientCase> PatientCases => Set<PatientCase>();
    public DbSet<AppointmentBooking> AppointmentBookings => Set<AppointmentBooking>();
    public DbSet<FleetVehicle> FleetVehicles => Set<FleetVehicle>();
    public DbSet<StoreItem> StoreItems => Set<StoreItem>();
    public DbSet<FacilityAsset> FacilityAssets => Set<FacilityAsset>();
    public DbSet<FacilityEvent> FacilityEvents => Set<FacilityEvent>();
    public DbSet<FacilityNeed> FacilityNeeds => Set<FacilityNeed>();
    public DbSet<PrisonInmate> PrisonInmates => Set<PrisonInmate>();
    public DbSet<PrisonStay> PrisonStays => Set<PrisonStay>();
    public DbSet<PrisonCase> PrisonCases => Set<PrisonCase>();
    public DbSet<PrisonCaseStep> PrisonCaseSteps => Set<PrisonCaseStep>();
    public DbSet<PrisonPharmacyItem> PrisonPharmacyItems => Set<PrisonPharmacyItem>();
    public DbSet<PrisonPrescription> PrisonPrescriptions => Set<PrisonPrescription>();
    public DbSet<PrisonDispense> PrisonDispenses => Set<PrisonDispense>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Facility>(entity =>
        {
            entity.ToTable("facilities");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasMaxLength(64);
            entity.Property(x => x.NameAr).HasMaxLength(200).IsRequired();
            entity.Property(x => x.ProvinceAr).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Type).HasConversion<int>();
            entity.HasIndex(x => x.Type);
        });

        builder.Entity<DailyAdmission>(entity =>
        {
            entity.ToTable("daily_admissions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FacilityId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(500);
            entity.Property(x => x.UpdatedBy).HasMaxLength(100).IsRequired();
            entity.HasIndex(x => new { x.FacilityId, x.Day }).IsUnique();
        });

        builder.Entity<PatientCase>(entity =>
        {
            entity.ToTable("patient_cases");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FacilityId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.CaseNumber).HasMaxLength(32).IsRequired();
            entity.Property(x => x.EntryType).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Category).HasMaxLength(16).IsRequired();
            entity.Property(x => x.SpecialType).HasMaxLength(64);
            entity.Property(x => x.SpecialNote).HasMaxLength(300);
            entity.Property(x => x.NationalId).HasMaxLength(11);
            entity.Property(x => x.PatientName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Residence).HasMaxLength(300);
            entity.Property(x => x.Governorate).HasMaxLength(64).IsRequired();
            entity.Property(x => x.AdminUnit).HasMaxLength(120);
            entity.Property(x => x.BookedClinic).HasMaxLength(80).IsRequired();
            entity.Property(x => x.CurrentClinic).HasMaxLength(80);
            entity.Property(x => x.CurrentDepartment).HasMaxLength(80);
            entity.Property(x => x.DepartmentNote).HasMaxLength(500);
            entity.Property(x => x.Status).HasConversion<int>();
            entity.Property(x => x.OpenedBy).HasMaxLength(100).IsRequired();
            entity.Property(x => x.UpdatedBy).HasMaxLength(100).IsRequired();
            entity.HasIndex(x => new { x.FacilityId, x.CaseNumber }).IsUnique();
            entity.HasIndex(x => new { x.FacilityId, x.Status });
            entity.HasIndex(x => new { x.FacilityId, x.NationalId });
            entity.HasIndex(x => new { x.FacilityId, x.CurrentDepartment, x.Status });
            entity.HasIndex(x => new { x.FacilityId, x.CalledAt });
        });

        builder.Entity<AppointmentBooking>(entity =>
        {
            entity.ToTable("appointment_bookings");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FacilityId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Code).HasMaxLength(8).IsRequired();
            entity.Property(x => x.Entity).HasMaxLength(80).IsRequired();
            entity.Property(x => x.AdminUnit).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Clinic).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Slot).HasMaxLength(8).IsRequired();
            entity.Property(x => x.PatientName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.NationalId).HasMaxLength(11);
            entity.Property(x => x.Governorate).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Residence).HasMaxLength(300);
            entity.Property(x => x.Category).HasMaxLength(16).IsRequired();
            entity.Property(x => x.SpecialType).HasMaxLength(64);
            entity.Property(x => x.Status).HasConversion<int>();
            entity.Property(x => x.BookedBy).HasMaxLength(120).IsRequired();
            entity.HasIndex(x => new { x.FacilityId, x.Code }).IsUnique();
            entity.HasIndex(x => new { x.FacilityId, x.Clinic, x.AppointmentDate, x.Status });
            entity.HasIndex(x => new { x.FacilityId, x.Entity, x.AdminUnit });
        });

        builder.Entity<FleetVehicle>(entity =>
        {
            entity.ToTable("fleet_vehicles");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FacilityId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Code).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Plate).HasMaxLength(32).IsRequired();
            entity.Property(x => x.VehicleType).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Equipment).HasMaxLength(300);
            entity.Property(x => x.DriverName).HasMaxLength(120);
            entity.Property(x => x.Status).HasConversion<int>();
            entity.Property(x => x.MissionNote).HasMaxLength(300);
            entity.Property(x => x.UpdatedBy).HasMaxLength(100).IsRequired();
            entity.HasIndex(x => new { x.FacilityId, x.Code }).IsUnique();
            entity.HasIndex(x => new { x.FacilityId, x.Status });
        });

        builder.Entity<StoreItem>(entity =>
        {
            entity.ToTable("store_items");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FacilityId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.StoreName).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Unit).HasMaxLength(40).IsRequired();
            entity.Property(x => x.UpdatedBy).HasMaxLength(100).IsRequired();
            entity.HasIndex(x => new { x.FacilityId, x.StoreName, x.Name }).IsUnique();
            entity.HasIndex(x => new { x.FacilityId, x.StoreName });
        });

        builder.Entity<FacilityAsset>(entity =>
        {
            entity.ToTable("facility_assets");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FacilityId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Code).HasMaxLength(32).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Department).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Note).HasMaxLength(400);
            entity.Property(x => x.Status).HasConversion<int>();
            entity.Property(x => x.UpdatedBy).HasMaxLength(100).IsRequired();
            entity.HasIndex(x => new { x.FacilityId, x.Code }).IsUnique();
            entity.HasIndex(x => new { x.FacilityId, x.Status });
            entity.HasIndex(x => new { x.FacilityId, x.Department });
        });

        builder.Entity<FacilityEvent>(entity =>
        {
            entity.ToTable("facility_events");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FacilityId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Kind).HasMaxLength(40).IsRequired();
            entity.Property(x => x.Severity).HasMaxLength(20).IsRequired();
            entity.Property(x => x.Details).HasMaxLength(500);
            entity.Property(x => x.Status).HasMaxLength(20).IsRequired();
            entity.Property(x => x.CreatedBy).HasMaxLength(100).IsRequired();
            entity.Property(x => x.ClosedBy).HasMaxLength(100);
            entity.HasIndex(x => new { x.FacilityId, x.Status, x.CreatedAt });
        });

        builder.Entity<FacilityNeed>(entity =>
        {
            entity.ToTable("facility_needs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FacilityId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Category).HasMaxLength(40).IsRequired();
            entity.Property(x => x.Priority).HasMaxLength(20).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(500);
            entity.Property(x => x.Status).HasMaxLength(20).IsRequired();
            entity.Property(x => x.CreatedBy).HasMaxLength(100).IsRequired();
            entity.Property(x => x.UpdatedBy).HasMaxLength(100);
            entity.HasIndex(x => new { x.FacilityId, x.Status, x.CreatedAt });
        });

        builder.Entity<PrisonInmate>(entity =>
        {
            entity.ToTable("prison_inmates");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FacilityId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.InmateNo).HasMaxLength(40).IsRequired();
            entity.Property(x => x.FileNo).HasMaxLength(40).IsRequired();
            entity.Property(x => x.Nationality).HasMaxLength(80).IsRequired();
            entity.Property(x => x.BirthPlace).HasMaxLength(120);
            entity.Property(x => x.BirthDate).HasMaxLength(20);
            entity.Property(x => x.BloodType).HasMaxLength(8).IsRequired();
            entity.Property(x => x.Sex).HasMaxLength(10).IsRequired();
            entity.Property(x => x.Marital).HasMaxLength(20).IsRequired();
            entity.Property(x => x.Place).HasMaxLength(120).IsRequired();
            entity.Property(x => x.EntryDate).HasMaxLength(20).IsRequired();
            entity.Property(x => x.ChronicCsv).HasMaxLength(400).IsRequired();
            entity.Property(x => x.Allergies).HasMaxLength(200).IsRequired();
            entity.HasIndex(x => new { x.FacilityId, x.InmateNo }).IsUnique();
            entity.HasIndex(x => new { x.FacilityId, x.FileNo });
            entity.HasMany(x => x.Stays).WithOne(x => x.Inmate!).HasForeignKey(x => x.InmateId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<PrisonStay>(entity =>
        {
            entity.ToTable("prison_stays");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Place).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Entry).HasMaxLength(20).IsRequired();
            entity.Property(x => x.Exit).HasMaxLength(20);
            entity.HasIndex(x => new { x.InmateId, x.SortOrder });
        });

        builder.Entity<PrisonCase>(entity =>
        {
            entity.ToTable("prison_cases");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FacilityId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.CaseCode).HasMaxLength(32).IsRequired();
            entity.Property(x => x.InmateNo).HasMaxLength(40).IsRequired();
            entity.Property(x => x.Triager).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Complaint).HasMaxLength(500).IsRequired();
            entity.Property(x => x.AtServiceId).HasMaxLength(20).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(20).IsRequired();
            entity.Property(x => x.HoldReason).HasMaxLength(300);
            entity.Property(x => x.OpenedTime).HasMaxLength(10).IsRequired();
            entity.HasIndex(x => new { x.FacilityId, x.CaseCode }).IsUnique();
            entity.HasIndex(x => new { x.FacilityId, x.Status, x.Day });
            entity.HasMany(x => x.Steps).WithOne(x => x.Case!).HasForeignKey(x => x.CaseId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<PrisonCaseStep>(entity =>
        {
            entity.ToTable("prison_case_steps");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Time).HasMaxLength(10).IsRequired();
            entity.Property(x => x.AtServiceId).HasMaxLength(20).IsRequired();
            entity.Property(x => x.By).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Diagnosis).HasMaxLength(300);
            entity.Property(x => x.Procedures).HasMaxLength(500);
            entity.Property(x => x.Meds).HasMaxLength(500);
            entity.Property(x => x.Note).HasMaxLength(300);
            entity.HasIndex(x => new { x.CaseId, x.SortOrder });
        });

        builder.Entity<PrisonPharmacyItem>(entity =>
        {
            entity.ToTable("prison_pharmacy_items");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FacilityId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Pharmacy).HasMaxLength(20).IsRequired();
            entity.Property(x => x.DrugName).HasMaxLength(120).IsRequired();
            entity.HasIndex(x => new { x.FacilityId, x.Pharmacy, x.DrugName }).IsUnique();
        });

        builder.Entity<PrisonPrescription>(entity =>
        {
            entity.ToTable("prison_prescriptions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FacilityId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Pharmacy).HasMaxLength(20).IsRequired();
            entity.Property(x => x.InmateNo).HasMaxLength(40).IsRequired();
            entity.Property(x => x.DrugName).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Doctor).HasMaxLength(100).IsRequired();
            entity.Property(x => x.CaseCode).HasMaxLength(32);
            entity.Property(x => x.Status).HasMaxLength(20).IsRequired();
            entity.Property(x => x.CreatedTime).HasMaxLength(10).IsRequired();
            entity.HasIndex(x => new { x.FacilityId, x.Pharmacy, x.Status, x.CreatedAt });
        });

        builder.Entity<PrisonDispense>(entity =>
        {
            entity.ToTable("prison_dispenses");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FacilityId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Pharmacy).HasMaxLength(20).IsRequired();
            entity.Property(x => x.InmateNo).HasMaxLength(40).IsRequired();
            entity.Property(x => x.DrugName).HasMaxLength(120).IsRequired();
            entity.Property(x => x.CaseCode).HasMaxLength(32);
            entity.Property(x => x.By).HasMaxLength(100);
            entity.Property(x => x.DispensedTime).HasMaxLength(10).IsRequired();
            entity.HasIndex(x => new { x.FacilityId, x.Pharmacy, x.CreatedAt });
        });

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(x => x.DisplayNameAr).HasMaxLength(200).IsRequired();
            entity.Property(x => x.FacilityId).HasMaxLength(64);
            entity.Property(x => x.HomePath).HasMaxLength(256);
            entity.HasIndex(x => x.FacilityId);
        });
    }
}
