using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicalServices.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FleetVehicles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "fleet_vehicles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FacilityId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Plate = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    VehicleType = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Equipment = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    DriverName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    OdometerKm = table.Column<int>(type: "integer", nullable: false),
                    LastServiceDate = table.Column<DateOnly>(type: "date", nullable: true),
                    MissionNote = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fleet_vehicles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_fleet_vehicles_FacilityId_Code",
                table: "fleet_vehicles",
                columns: new[] { "FacilityId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_fleet_vehicles_FacilityId_Status",
                table: "fleet_vehicles",
                columns: new[] { "FacilityId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "fleet_vehicles");
        }
    }
}
