using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicalServices.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FacilityAssets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "facility_assets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FacilityId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Department = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Count = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Note = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_facility_assets", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_facility_assets_FacilityId_Code",
                table: "facility_assets",
                columns: new[] { "FacilityId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_facility_assets_FacilityId_Department",
                table: "facility_assets",
                columns: new[] { "FacilityId", "Department" });

            migrationBuilder.CreateIndex(
                name: "IX_facility_assets_FacilityId_Status",
                table: "facility_assets",
                columns: new[] { "FacilityId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "facility_assets");
        }
    }
}
