using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace eDom.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SyncModelForPostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "APP_DASH_LAYOUT",
                schema: "HICT",
                columns: table => new
                {
                    DASH_ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DASH_USER_CODICE = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DASH_LAYOUT_JSON = table.Column<string>(type: "text", nullable: false),
                    DASH_UPDATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_APP_DASH_LAYOUT", x => x.DASH_ID);
                });

            migrationBuilder.CreateIndex(
                name: "UX_DASH_USER_CODICE",
                schema: "HICT",
                table: "APP_DASH_LAYOUT",
                column: "DASH_USER_CODICE",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "APP_DASH_LAYOUT",
                schema: "HICT");
        }
    }
}
