using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AstroTBotService.Migrations
{
    /// <inheritdoc />
    public partial class ChangePersonsRefs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AstroPersons_AstroPersons_ParentUserId",
                table: "AstroPersons");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "AstroPersons");

            migrationBuilder.DropColumn(
                name: "HouseSystem",
                table: "AstroPersons");

            migrationBuilder.DropColumn(
                name: "Language",
                table: "AstroPersons");

            migrationBuilder.CreateTable(
                name: "AstroUsers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    Language = table.Column<string>(type: "text", nullable: true),
                    HouseSystem = table.Column<int>(type: "integer", nullable: false),
                    BirthDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    GmtOffset = table.Column<TimeSpan>(type: "interval", nullable: true),
                    Longitude = table.Column<double>(type: "double precision", nullable: true),
                    Latitude = table.Column<double>(type: "double precision", nullable: true),
                    ParentUserId = table.Column<long>(type: "bigint", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true),
                    IsChosen = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AstroUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AstroUsers_AstroUsers_ParentUserId",
                        column: x => x.ParentUserId,
                        principalTable: "AstroUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AstroUsers_ParentUserId",
                table: "AstroUsers",
                column: "ParentUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AstroPersons_AstroUsers_ParentUserId",
                table: "AstroPersons",
                column: "ParentUserId",
                principalTable: "AstroUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AstroPersons_AstroUsers_ParentUserId",
                table: "AstroPersons");

            migrationBuilder.DropTable(
                name: "AstroUsers");

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "AstroPersons",
                type: "character varying(13)",
                maxLength: 13,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "HouseSystem",
                table: "AstroPersons",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Language",
                table: "AstroPersons",
                type: "text",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AstroPersons_AstroPersons_ParentUserId",
                table: "AstroPersons",
                column: "ParentUserId",
                principalTable: "AstroPersons",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
