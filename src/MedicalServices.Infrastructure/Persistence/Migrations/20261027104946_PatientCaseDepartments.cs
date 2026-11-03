using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicalServices.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PatientCaseDepartments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CurrentDepartment",
                table: "patient_cases",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DepartmentNote",
                table: "patient_cases",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_patient_cases_FacilityId_CurrentDepartment_Status",
                table: "patient_cases",
                columns: new[] { "FacilityId", "CurrentDepartment", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_patient_cases_FacilityId_CurrentDepartment_Status",
                table: "patient_cases");

            migrationBuilder.DropColumn(
                name: "CurrentDepartment",
                table: "patient_cases");

            migrationBuilder.DropColumn(
                name: "DepartmentNote",
                table: "patient_cases");
        }
    }
}
