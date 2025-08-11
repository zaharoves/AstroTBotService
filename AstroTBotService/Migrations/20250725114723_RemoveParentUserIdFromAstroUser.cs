using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AstroTBotService.Migrations
{
    /// <inheritdoc />
    public partial class RemoveParentUserIdFromAstroUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ParentUserId",
                table: "AstroUsers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ParentUserId",
                table: "AstroUsers",
                type: "bigint",
                nullable: true);
        }
    }
}
