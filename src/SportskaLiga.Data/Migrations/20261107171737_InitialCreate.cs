using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SportskaLiga.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    ImePrezime = table.Column<string>(type: "text", nullable: true),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Momcadi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Naziv = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Grad = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Igraliste = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    GodinaOsnutka = table.Column<int>(type: "integer", nullable: true),
                    KontaktEmail = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Momcadi", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sezone",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Naziv = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DatumPocetka = table.Column<DateOnly>(type: "date", nullable: false),
                    DatumZavrsetka = table.Column<DateOnly>(type: "date", nullable: false),
                    BodoviZaPobjedu = table.Column<int>(type: "integer", nullable: false),
                    BodoviZaNerijeseno = table.Column<int>(type: "integer", nullable: false),
                    Aktivna = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sezone", x => x.Id);
                    table.CheckConstraint("CK_Sezona_Razdoblje", "\"DatumZavrsetka\" >= \"DatumPocetka\"");
                });

            migrationBuilder.CreateTable(
                name: "Suci",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Ime = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Prezime = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Telefon = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Suci", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Igraci",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Ime = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Prezime = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DatumRodenja = table.Column<DateOnly>(type: "date", nullable: false),
                    BrojDresa = table.Column<int>(type: "integer", nullable: false),
                    Pozicija = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    MomcadId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Igraci", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Igraci_Momcadi_MomcadId",
                        column: x => x.MomcadId,
                        principalTable: "Momcadi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SezoneMomcadi",
                columns: table => new
                {
                    SezonaId = table.Column<int>(type: "integer", nullable: false),
                    MomcadId = table.Column<int>(type: "integer", nullable: false),
                    DatumPrijave = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SezoneMomcadi", x => new { x.SezonaId, x.MomcadId });
                    table.ForeignKey(
                        name: "FK_SezoneMomcadi_Momcadi_MomcadId",
                        column: x => x.MomcadId,
                        principalTable: "Momcadi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SezoneMomcadi_Sezone_SezonaId",
                        column: x => x.SezonaId,
                        principalTable: "Sezone",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Utakmice",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SezonaId = table.Column<int>(type: "integer", nullable: false),
                    Kolo = table.Column<int>(type: "integer", nullable: false),
                    Pocetak = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Lokacija = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DomacinId = table.Column<int>(type: "integer", nullable: false),
                    GostId = table.Column<int>(type: "integer", nullable: false),
                    GoloviDomacin = table.Column<int>(type: "integer", nullable: true),
                    GoloviGost = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SudacId = table.Column<int>(type: "integer", nullable: true),
                    DelegatId = table.Column<string>(type: "text", nullable: true),
                    Napomena = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Utakmice", x => x.Id);
                    table.CheckConstraint("CK_Utakmica_Golovi", "\"GoloviDomacin\" >= 0 AND \"GoloviGost\" >= 0");
                    table.CheckConstraint("CK_Utakmica_RazliciteMomcadi", "\"DomacinId\" <> \"GostId\"");
                    table.ForeignKey(
                        name: "FK_Utakmice_AspNetUsers_DelegatId",
                        column: x => x.DelegatId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Utakmice_Momcadi_DomacinId",
                        column: x => x.DomacinId,
                        principalTable: "Momcadi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Utakmice_Momcadi_GostId",
                        column: x => x.GostId,
                        principalTable: "Momcadi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Utakmice_Sezone_SezonaId",
                        column: x => x.SezonaId,
                        principalTable: "Sezone",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Utakmice_Suci_SudacId",
                        column: x => x.SudacId,
                        principalTable: "Suci",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Golovi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UtakmicaId = table.Column<int>(type: "integer", nullable: false),
                    IgracId = table.Column<int>(type: "integer", nullable: false),
                    MomcadId = table.Column<int>(type: "integer", nullable: false),
                    Minuta = table.Column<int>(type: "integer", nullable: false),
                    Autogol = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Golovi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Golovi_Igraci_IgracId",
                        column: x => x.IgracId,
                        principalTable: "Igraci",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Golovi_Momcadi_MomcadId",
                        column: x => x.MomcadId,
                        principalTable: "Momcadi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Golovi_Utakmice_UtakmicaId",
                        column: x => x.UtakmicaId,
                        principalTable: "Utakmice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Golovi_IgracId",
                table: "Golovi",
                column: "IgracId");

            migrationBuilder.CreateIndex(
                name: "IX_Golovi_MomcadId",
                table: "Golovi",
                column: "MomcadId");

            migrationBuilder.CreateIndex(
                name: "IX_Golovi_UtakmicaId",
                table: "Golovi",
                column: "UtakmicaId");

            migrationBuilder.CreateIndex(
                name: "IX_Igraci_MomcadId_BrojDresa",
                table: "Igraci",
                columns: new[] { "MomcadId", "BrojDresa" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Igraci_Prezime_Ime",
                table: "Igraci",
                columns: new[] { "Prezime", "Ime" });

            migrationBuilder.CreateIndex(
                name: "IX_Momcadi_Naziv",
                table: "Momcadi",
                column: "Naziv",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sezone_Naziv",
                table: "Sezone",
                column: "Naziv",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SezoneMomcadi_MomcadId",
                table: "SezoneMomcadi",
                column: "MomcadId");

            migrationBuilder.CreateIndex(
                name: "IX_Utakmice_DelegatId",
                table: "Utakmice",
                column: "DelegatId");

            migrationBuilder.CreateIndex(
                name: "IX_Utakmice_DomacinId",
                table: "Utakmice",
                column: "DomacinId");

            migrationBuilder.CreateIndex(
                name: "IX_Utakmice_GostId",
                table: "Utakmice",
                column: "GostId");

            migrationBuilder.CreateIndex(
                name: "IX_Utakmice_SezonaId_Kolo",
                table: "Utakmice",
                columns: new[] { "SezonaId", "Kolo" });

            migrationBuilder.CreateIndex(
                name: "IX_Utakmice_SudacId",
                table: "Utakmice",
                column: "SudacId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "Golovi");

            migrationBuilder.DropTable(
                name: "SezoneMomcadi");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Igraci");

            migrationBuilder.DropTable(
                name: "Utakmice");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Momcadi");

            migrationBuilder.DropTable(
                name: "Sezone");

            migrationBuilder.DropTable(
                name: "Suci");
        }
    }
}
