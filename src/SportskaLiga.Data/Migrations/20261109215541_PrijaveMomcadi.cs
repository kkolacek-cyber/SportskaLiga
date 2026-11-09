using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SportskaLiga.Data.Migrations
{
    /// <inheritdoc />
    public partial class PrijaveMomcadi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PrijaveMomcadi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Kod = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    SezonaId = table.Column<int>(type: "integer", nullable: false),
                    NazivMomcadi = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Grad = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Igraliste = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    KontaktOsoba = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    KontaktEmail = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    KontaktTelefon = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    BrojIgraca = table.Column<int>(type: "integer", nullable: false),
                    Napomena = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DatumPodnosenja = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DatumObrade = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OdgovorLige = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    MomcadId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrijaveMomcadi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrijaveMomcadi_Momcadi_MomcadId",
                        column: x => x.MomcadId,
                        principalTable: "Momcadi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PrijaveMomcadi_Sezone_SezonaId",
                        column: x => x.SezonaId,
                        principalTable: "Sezone",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PrijaveMomcadi_Kod",
                table: "PrijaveMomcadi",
                column: "Kod",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PrijaveMomcadi_MomcadId",
                table: "PrijaveMomcadi",
                column: "MomcadId");

            migrationBuilder.CreateIndex(
                name: "IX_PrijaveMomcadi_SezonaId",
                table: "PrijaveMomcadi",
                column: "SezonaId");

            migrationBuilder.CreateIndex(
                name: "IX_PrijaveMomcadi_Status",
                table: "PrijaveMomcadi",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PrijaveMomcadi");
        }
    }
}
