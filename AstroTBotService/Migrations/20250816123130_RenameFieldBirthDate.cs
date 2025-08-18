using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AstroTBotService.Migrations
{
    /// <inheritdoc />
    public partial class RenameFieldBirthDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BirthDate",
                table: "AstroUsers",
                newName: "UtcBirthDate");

            migrationBuilder.RenameColumn(
                name: "BirthDate",
                table: "AstroPersons",
                newName: "UtcBirthDate");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "UsersStages",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UtcBirthDate",
                table: "AstroUsers",
                newName: "BirthDate");

            migrationBuilder.RenameColumn(
                name: "UtcBirthDate",
                table: "AstroPersons",
                newName: "BirthDate");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "UsersStages",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
        }
    }
}
