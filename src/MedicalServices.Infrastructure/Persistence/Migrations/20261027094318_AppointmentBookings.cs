using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicalServices.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AppointmentBookings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "appointment_bookings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FacilityId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Code = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    Entity = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    AdminUnit = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Clinic = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    AppointmentDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Slot = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    PatientName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NationalId = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: true),
                    Age = table.Column<int>(type: "integer", nullable: true),
                    Governorate = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Residence = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Category = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    SpecialType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    BookedBy = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_appointment_bookings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_appointment_bookings_FacilityId_Clinic_AppointmentDate_Stat~",
                table: "appointment_bookings",
                columns: new[] { "FacilityId", "Clinic", "AppointmentDate", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_appointment_bookings_FacilityId_Code",
                table: "appointment_bookings",
                columns: new[] { "FacilityId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_appointment_bookings_FacilityId_Entity_AdminUnit",
                table: "appointment_bookings",
                columns: new[] { "FacilityId", "Entity", "AdminUnit" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "appointment_bookings");
        }
    }
}
