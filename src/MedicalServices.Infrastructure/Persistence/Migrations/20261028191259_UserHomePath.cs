using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicalServices.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UserHomePath : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HomePath",
                table: "AspNetUsers",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HomePath",
                table: "AspNetUsers");
        }
    }
}
