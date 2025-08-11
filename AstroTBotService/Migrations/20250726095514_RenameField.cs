using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AstroTBotService.Migrations
{
    /// <inheritdoc />
    public partial class RenameField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsChoosen",
                table: "AstroUsers",
                newName: "IsChosen");

            migrationBuilder.RenameColumn(
                name: "IsChoosen",
                table: "AstroPersons",
                newName: "IsChosen");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsChosen",
                table: "AstroUsers",
                newName: "IsChoosen");

            migrationBuilder.RenameColumn(
                name: "IsChosen",
                table: "AstroPersons",
                newName: "IsChoosen");
        }
    }
}
