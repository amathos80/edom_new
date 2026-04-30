using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace eDom.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSiSismess : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SI_SISMESS",
                schema: "HICT",
                columns: table => new
                {
                    SISM_ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SISM_CLASSE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SISM_NOME = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SISM_DESCR = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    SISM_LINGUA = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    SISM_CUSTOM01 = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    SISM_CUSTOM02 = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    SISM_CUSTOM03 = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    SISM_CUSTOM04 = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    SISM_CUSTOM05 = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    SISM_F_ATTIVO = table.Column<short>(type: "smallint", nullable: false),
                    SISM_UTINS = table.Column<int>(type: "integer", nullable: false),
                    SISM_DTINS = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SISM_UTMOD = table.Column<int>(type: "integer", nullable: true),
                    SISM_DTMOD = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SISM_VERSION = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SI_SISMESS", x => x.SISM_ID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SI_SISMESS",
                schema: "HICT");
        }
    }
}
