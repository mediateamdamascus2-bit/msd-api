using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicalServices.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class HasiaPrisonOps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "prison_cases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FacilityId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CaseCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    InmateNo = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Triager = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Complaint = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    AtServiceId = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    HoldReason = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    OpenedTime = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Day = table.Column<DateOnly>(type: "date", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prison_cases", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "prison_dispenses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FacilityId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Pharmacy = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    InmateNo = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    DrugName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    CaseCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    By = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DispensedTime = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prison_dispenses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "prison_inmates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FacilityId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    InmateNo = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    FileNo = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Nationality = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    BirthPlace = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    BirthDate = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    BloodType = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    Sex = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Marital = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Place = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    EntryDate = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ChronicCsv = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    Allergies = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    RecordJson = table.Column<string>(type: "text", nullable: true),
                    DiagnosesJson = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prison_inmates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "prison_pharmacy_items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FacilityId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Pharmacy = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DrugName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prison_pharmacy_items", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "prison_prescriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FacilityId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Pharmacy = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    InmateNo = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    DrugName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Doctor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CaseCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedTime = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prison_prescriptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "prison_case_steps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    Time = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    AtServiceId = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    By = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Diagnosis = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Procedures = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Meds = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Note = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prison_case_steps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_prison_case_steps_prison_cases_CaseId",
                        column: x => x.CaseId,
                        principalTable: "prison_cases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "prison_stays",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InmateId = table.Column<Guid>(type: "uuid", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    Place = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Entry = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Exit = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prison_stays", x => x.Id);
                    table.ForeignKey(
                        name: "FK_prison_stays_prison_inmates_InmateId",
                        column: x => x.InmateId,
                        principalTable: "prison_inmates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_prison_case_steps_CaseId_SortOrder",
                table: "prison_case_steps",
                columns: new[] { "CaseId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_prison_cases_FacilityId_CaseCode",
                table: "prison_cases",
                columns: new[] { "FacilityId", "CaseCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_prison_cases_FacilityId_Status_Day",
                table: "prison_cases",
                columns: new[] { "FacilityId", "Status", "Day" });

            migrationBuilder.CreateIndex(
                name: "IX_prison_dispenses_FacilityId_Pharmacy_CreatedAt",
                table: "prison_dispenses",
                columns: new[] { "FacilityId", "Pharmacy", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_prison_inmates_FacilityId_FileNo",
                table: "prison_inmates",
                columns: new[] { "FacilityId", "FileNo" });

            migrationBuilder.CreateIndex(
                name: "IX_prison_inmates_FacilityId_InmateNo",
                table: "prison_inmates",
                columns: new[] { "FacilityId", "InmateNo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_prison_pharmacy_items_FacilityId_Pharmacy_DrugName",
                table: "prison_pharmacy_items",
                columns: new[] { "FacilityId", "Pharmacy", "DrugName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_prison_prescriptions_FacilityId_Pharmacy_Status_CreatedAt",
                table: "prison_prescriptions",
                columns: new[] { "FacilityId", "Pharmacy", "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_prison_stays_InmateId_SortOrder",
                table: "prison_stays",
                columns: new[] { "InmateId", "SortOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "prison_case_steps");

            migrationBuilder.DropTable(
                name: "prison_dispenses");

            migrationBuilder.DropTable(
                name: "prison_pharmacy_items");

            migrationBuilder.DropTable(
                name: "prison_prescriptions");

            migrationBuilder.DropTable(
                name: "prison_stays");

            migrationBuilder.DropTable(
                name: "prison_cases");

            migrationBuilder.DropTable(
                name: "prison_inmates");
        }
    }
}
