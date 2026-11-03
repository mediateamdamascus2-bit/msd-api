using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicalServices.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PatientCases : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "patient_cases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FacilityId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CaseNumber = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    EntryType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Category = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    SpecialType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SpecialNote = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    NationalId = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: true),
                    PatientName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Age = table.Column<int>(type: "integer", nullable: true),
                    Residence = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Governorate = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    AdminUnit = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    BookedClinic = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    CurrentClinic = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    OpenedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    OpenedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_patient_cases", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_patient_cases_FacilityId_CaseNumber",
                table: "patient_cases",
                columns: new[] { "FacilityId", "CaseNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_patient_cases_FacilityId_NationalId",
                table: "patient_cases",
                columns: new[] { "FacilityId", "NationalId" });

            migrationBuilder.CreateIndex(
                name: "IX_patient_cases_FacilityId_Status",
                table: "patient_cases",
                columns: new[] { "FacilityId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "patient_cases");
        }
    }
}
