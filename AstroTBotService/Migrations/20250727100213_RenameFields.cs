using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AstroTBotService.Migrations
{
    /// <inheritdoc />
    public partial class RenameFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "GmtOffset",
                table: "AstroUsers",
                newName: "TimeZoneOffset");

            migrationBuilder.RenameColumn(
                name: "GmtOffset",
                table: "AstroPersons",
                newName: "TimeZoneOffset");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TimeZoneOffset",
                table: "AstroUsers",
                newName: "GmtOffset");

            migrationBuilder.RenameColumn(
                name: "TimeZoneOffset",
                table: "AstroPersons",
                newName: "GmtOffset");
        }
    }
}
