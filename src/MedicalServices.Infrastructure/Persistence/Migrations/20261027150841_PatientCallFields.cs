using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicalServices.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PatientCallFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CallCount",
                table: "patient_cases",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CalledAt",
                table: "patient_cases",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_patient_cases_FacilityId_CalledAt",
                table: "patient_cases",
                columns: new[] { "FacilityId", "CalledAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_patient_cases_FacilityId_CalledAt",
                table: "patient_cases");

            migrationBuilder.DropColumn(
                name: "CallCount",
                table: "patient_cases");

            migrationBuilder.DropColumn(
                name: "CalledAt",
                table: "patient_cases");
        }
    }
}
