using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace eDom.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProcedureAndRuoloFunzione : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SI_PROCEDURE",
                schema: "HICT",
                columns: table => new
                {
                    PROC_ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PROC_CODICE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PROC_DESCR = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PROC_UTINS = table.Column<int>(type: "integer", nullable: false),
                    PROC_DTINS = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PROC_UTMOD = table.Column<int>(type: "integer", nullable: true),
                    PROC_DTMOD = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PROC_VERSION = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PROC_DBSCHEMA = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PROC_DBPWD = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PROC_VISIBILE = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SI_PROCEDURE", x => x.PROC_ID);
                });

            migrationBuilder.CreateTable(
                name: "SI_FUNZIONI",
                schema: "HICT",
                columns: table => new
                {
                    FUNZ_ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FUNZ_PROC_ID = table.Column<int>(type: "integer", nullable: false),
                    FUNZ_CODICE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FUNZ_DESCR = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FUNZ_PARENT = table.Column<int>(type: "integer", nullable: true),
                    FUNZ_UTINS = table.Column<int>(type: "integer", nullable: false),
                    FUNZ_DTINS = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FUNZ_UTMOD = table.Column<int>(type: "integer", nullable: true),
                    FUNZ_DTMOD = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FUNZ_VERSION = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FUNZ_SORT = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SI_FUNZIONI", x => x.FUNZ_ID);
                    table.ForeignKey(
                        name: "FK_SI_FUNZIONI_SI_FUNZIONI_FUNZ_PARENT",
                        column: x => x.FUNZ_PARENT,
                        principalSchema: "HICT",
                        principalTable: "SI_FUNZIONI",
                        principalColumn: "FUNZ_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SI_FUNZIONI_SI_PROCEDURE_FUNZ_PROC_ID",
                        column: x => x.FUNZ_PROC_ID,
                        principalSchema: "HICT",
                        principalTable: "SI_PROCEDURE",
                        principalColumn: "PROC_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SI_RUOLFUNZ",
                schema: "HICT",
                columns: table => new
                {
                    RUFU_ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RUFU_RUOL_ID = table.Column<int>(type: "integer", nullable: false),
                    RUFU_FUNZ_ID = table.Column<int>(type: "integer", nullable: false),
                    RUFU_RUOL_PROC_ID = table.Column<int>(type: "integer", nullable: false),
                    RUFU_FUNZ_PROC_ID = table.Column<int>(type: "integer", nullable: false),
                    RUFU_UTINS = table.Column<int>(type: "integer", nullable: false),
                    RUFU_DTINS = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RUFU_UTMOD = table.Column<int>(type: "integer", nullable: true),
                    RUFU_DTMOD = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RUFU_VERSION = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SI_RUOLFUNZ", x => x.RUFU_ID);
                    table.ForeignKey(
                        name: "FK_SI_RUOLFUNZ_SI_FUNZIONI_RUFU_FUNZ_ID",
                        column: x => x.RUFU_FUNZ_ID,
                        principalSchema: "HICT",
                        principalTable: "SI_FUNZIONI",
                        principalColumn: "FUNZ_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SI_RUOLFUNZ_SI_PROCEDURE_RUFU_FUNZ_PROC_ID",
                        column: x => x.RUFU_FUNZ_PROC_ID,
                        principalSchema: "HICT",
                        principalTable: "SI_PROCEDURE",
                        principalColumn: "PROC_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SI_RUOLFUNZ_SI_PROCEDURE_RUFU_RUOL_PROC_ID",
                        column: x => x.RUFU_RUOL_PROC_ID,
                        principalSchema: "HICT",
                        principalTable: "SI_PROCEDURE",
                        principalColumn: "PROC_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SI_RUOLFUNZ_SI_RUOLI_RUFU_RUOL_ID",
                        column: x => x.RUFU_RUOL_ID,
                        principalSchema: "HICT",
                        principalTable: "SI_RUOLI",
                        principalColumn: "RUOL_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            // Seed placeholder rows in SI_PROCEDURE for any RUOL_PROC_ID values in SI_RUOLI
            // that reference procedure IDs not yet present (legacy data compatibility)
            migrationBuilder.Sql(@"
                INSERT INTO ""HICT"".""SI_PROCEDURE"" (""PROC_ID"", ""PROC_CODICE"", ""PROC_DESCR"", ""PROC_UTINS"", ""PROC_DTINS"", ""PROC_VISIBILE"")
                SELECT DISTINCT r.""RUOL_PROC_ID"", 'LEGACY_' || r.""RUOL_PROC_ID"", 'Migrated from legacy data', 1, NOW(), 1
                FROM ""HICT"".""SI_RUOLI"" r
                WHERE r.""RUOL_PROC_ID"" IS NOT NULL
                  AND NOT EXISTS (
                      SELECT 1 FROM ""HICT"".""SI_PROCEDURE"" p WHERE p.""PROC_ID"" = r.""RUOL_PROC_ID""
                  );");

            migrationBuilder.CreateIndex(
                name: "IX_SI_RUOLI_RUOL_PROC_ID",
                schema: "HICT",
                table: "SI_RUOLI",
                column: "RUOL_PROC_ID");

            migrationBuilder.CreateIndex(
                name: "IX_SI_FUNZIONI_FUNZ_PARENT",
                schema: "HICT",
                table: "SI_FUNZIONI",
                column: "FUNZ_PARENT");

            migrationBuilder.CreateIndex(
                name: "IX_SI_FUNZIONI_FUNZ_PROC_ID",
                schema: "HICT",
                table: "SI_FUNZIONI",
                column: "FUNZ_PROC_ID");

            migrationBuilder.CreateIndex(
                name: "IX_SI_RUOLFUNZ_RUFU_FUNZ_ID",
                schema: "HICT",
                table: "SI_RUOLFUNZ",
                column: "RUFU_FUNZ_ID");

            migrationBuilder.CreateIndex(
                name: "IX_SI_RUOLFUNZ_RUFU_FUNZ_PROC_ID",
                schema: "HICT",
                table: "SI_RUOLFUNZ",
                column: "RUFU_FUNZ_PROC_ID");

            migrationBuilder.CreateIndex(
                name: "IX_SI_RUOLFUNZ_RUFU_RUOL_ID",
                schema: "HICT",
                table: "SI_RUOLFUNZ",
                column: "RUFU_RUOL_ID");

            migrationBuilder.CreateIndex(
                name: "IX_SI_RUOLFUNZ_RUFU_RUOL_PROC_ID",
                schema: "HICT",
                table: "SI_RUOLFUNZ",
                column: "RUFU_RUOL_PROC_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_SI_RUOLI_SI_PROCEDURE_RUOL_PROC_ID",
                schema: "HICT",
                table: "SI_RUOLI",
                column: "RUOL_PROC_ID",
                principalSchema: "HICT",
                principalTable: "SI_PROCEDURE",
                principalColumn: "PROC_ID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SI_RUOLI_SI_PROCEDURE_RUOL_PROC_ID",
                schema: "HICT",
                table: "SI_RUOLI");

            migrationBuilder.DropTable(
                name: "SI_RUOLFUNZ",
                schema: "HICT");

            migrationBuilder.DropTable(
                name: "SI_FUNZIONI",
                schema: "HICT");

            migrationBuilder.DropTable(
                name: "SI_PROCEDURE",
                schema: "HICT");

            migrationBuilder.DropIndex(
                name: "IX_SI_RUOLI_RUOL_PROC_ID",
                schema: "HICT",
                table: "SI_RUOLI");
        }
    }
}
