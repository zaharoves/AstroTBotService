using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AstroTBotService.Migrations
{
    /// <inheritdoc />
    public partial class AddUserEmailField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsChoosen",
                table: "AstroUsers",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "AstroUsers",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsChoosen",
                table: "AstroUsers");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "AstroUsers");
        }
    }
}
