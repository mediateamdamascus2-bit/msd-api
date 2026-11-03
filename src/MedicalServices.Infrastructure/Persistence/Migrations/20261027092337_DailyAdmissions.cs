using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicalServices.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DailyAdmissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "daily_admissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FacilityId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Day = table.Column<DateOnly>(type: "date", nullable: false),
                    AdmissionsCount = table.Column<int>(type: "integer", nullable: false),
                    OccupiedBeds = table.Column<int>(type: "integer", nullable: false),
                    AvailableBeds = table.Column<int>(type: "integer", nullable: false),
                    TransfersCount = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_daily_admissions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_daily_admissions_FacilityId_Day",
                table: "daily_admissions",
                columns: new[] { "FacilityId", "Day" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "daily_admissions");
        }
    }
}
