using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AstroTBotService.Migrations
{
    /// <inheritdoc />
    public partial class InitMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AstroUsers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    BirthDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    GmtOffset = table.Column<TimeSpan>(type: "interval", nullable: true),
                    Language = table.Column<string>(type: "text", nullable: true),
                    HouseSystem = table.Column<int>(type: "integer", nullable: false),
                    Longitude = table.Column<double>(type: "double precision", nullable: true),
                    Latitude = table.Column<double>(type: "double precision", nullable: true),
                    ParentUserId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AstroUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AstroUsers_AstroUsers_ParentUserId",
                        column: x => x.ParentUserId,
                        principalTable: "AstroUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Ephemerises",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SunAngles = table.Column<double>(type: "double precision", nullable: true),
                    MoonAngles = table.Column<double>(type: "double precision", nullable: true),
                    MercuryAngles = table.Column<double>(type: "double precision", nullable: true),
                    VenusAngles = table.Column<double>(type: "double precision", nullable: true),
                    MarsAngles = table.Column<double>(type: "double precision", nullable: true),
                    JupiterAngles = table.Column<double>(type: "double precision", nullable: true),
                    SaturnAngles = table.Column<double>(type: "double precision", nullable: true),
                    UranAngles = table.Column<double>(type: "double precision", nullable: true),
                    NeptuneAngles = table.Column<double>(type: "double precision", nullable: true),
                    PlutoAngles = table.Column<double>(type: "double precision", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ephemerises", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UsersStages",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Stage = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsersStages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AstroUsers_ParentUserId",
                table: "AstroUsers",
                column: "ParentUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AstroUsers");

            migrationBuilder.DropTable(
                name: "Ephemerises");

            migrationBuilder.DropTable(
                name: "UsersStages");
        }
    }
}
