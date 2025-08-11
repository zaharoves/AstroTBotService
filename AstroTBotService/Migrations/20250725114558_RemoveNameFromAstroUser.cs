using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AstroTBotService.Migrations
{
    /// <inheritdoc />
    public partial class RemoveNameFromAstroUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "AstroUsers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "AstroUsers",
                type: "text",
                nullable: true);
        }
    }
}
