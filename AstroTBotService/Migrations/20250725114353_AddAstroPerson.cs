using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AstroTBotService.Migrations
{
    /// <inheritdoc />
    public partial class AddAstroPerson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AstroUsers_AstroUsers_ParentUserId",
                table: "AstroUsers");

            migrationBuilder.DropIndex(
                name: "IX_AstroUsers_ParentUserId",
                table: "AstroUsers");

            migrationBuilder.CreateTable(
                name: "AstroPersons",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BirthDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    GmtOffset = table.Column<TimeSpan>(type: "interval", nullable: true),
                    Longitude = table.Column<double>(type: "double precision", nullable: true),
                    Latitude = table.Column<double>(type: "double precision", nullable: true),
                    ParentUserId = table.Column<long>(type: "bigint", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true),
                    IsChoosen = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AstroPersons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AstroPersons_AstroUsers_ParentUserId",
                        column: x => x.ParentUserId,
                        principalTable: "AstroUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AstroPersons_ParentUserId",
                table: "AstroPersons",
                column: "ParentUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AstroPersons");

            migrationBuilder.CreateIndex(
                name: "IX_AstroUsers_ParentUserId",
                table: "AstroUsers",
                column: "ParentUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AstroUsers_AstroUsers_ParentUserId",
                table: "AstroUsers",
                column: "ParentUserId",
                principalTable: "AstroUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
