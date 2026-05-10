using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace eDom.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSsPuaManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "RUOL_ID",
                schema: "HICT",
                table: "SI_RUOLI",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "RUFU_ID",
                schema: "HICT",
                table: "SI_RUOLFUNZ",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "LOAC_ID",
                schema: "HICT",
                table: "SI_LOGACC",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<long>(
                name: "AULO_ID",
                schema: "HICT",
                table: "SI_AUDIT_LOG",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "PAZI_ID",
                schema: "HICT",
                table: "CO_PAZIENTI",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "DASH_ID",
                schema: "HICT",
                table: "APP_DASH_LAYOUT",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.CreateTable(
                name: "SS_PUA",
                schema: "HICT",
                columns: table => new
                {
                    PU01_ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PU01_NUMPUA_ID = table.Column<int>(type: "integer", nullable: false),
                    PU01_NUMERO = table.Column<int>(type: "integer", nullable: false),
                    PU01_DATA = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PU01_ELTG_AREAINT = table.Column<int>(type: "integer", nullable: false),
                    PU01_PAZI_ID = table.Column<int>(type: "integer", nullable: false),
                    PU01_PAZI_COGNOME = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PU01_PAZI_NOME = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PU01_PAZI_CODFISC = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    PU01_ELTG_ACCESSO = table.Column<int>(type: "integer", nullable: false),
                    PU01_ACCESSO_NOTE = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    PU01_ELTG_MOTIVO = table.Column<int>(type: "integer", nullable: true),
                    PU01_MOTIVO_NOTE = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    PU01_TCON_RICHIESTA = table.Column<int>(type: "integer", nullable: false),
                    PU01_RICH_ALTRO = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    PU01_TCON_ESITO = table.Column<int>(type: "integer", nullable: false),
                    PU01_ESITO_NOTE = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    PU01_F_URGENTE = table.Column<short>(type: "smallint", nullable: false),
                    PU01_TCON_ORIGINE = table.Column<int>(type: "integer", nullable: false),
                    PU01_DTAVVIO = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PU01_DTCHIUSURA = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PU01_ELTG_MOTCHIU = table.Column<int>(type: "integer", nullable: true),
                    PU01_F_ATT = table.Column<short>(type: "smallint", nullable: false),
                    PU01_DTDISATT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PU01_UTINS = table.Column<int>(type: "integer", nullable: false),
                    PU01_DTINS = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PU01_UTMOD = table.Column<int>(type: "integer", nullable: true),
                    PU01_DTMOD = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PU01_VERSION = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SS_PUA", x => x.PU01_ID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SS_PUA",
                schema: "HICT");

            migrationBuilder.AlterColumn<int>(
                name: "RUOL_ID",
                schema: "HICT",
                table: "SI_RUOLI",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn);

            migrationBuilder.AlterColumn<int>(
                name: "RUFU_ID",
                schema: "HICT",
                table: "SI_RUOLFUNZ",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn);

            migrationBuilder.AlterColumn<int>(
                name: "LOAC_ID",
                schema: "HICT",
                table: "SI_LOGACC",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn);

            migrationBuilder.AlterColumn<long>(
                name: "AULO_ID",
                schema: "HICT",
                table: "SI_AUDIT_LOG",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn);

            migrationBuilder.AlterColumn<int>(
                name: "PAZI_ID",
                schema: "HICT",
                table: "CO_PAZIENTI",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn);

            migrationBuilder.AlterColumn<int>(
                name: "DASH_ID",
                schema: "HICT",
                table: "APP_DASH_LAYOUT",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn);
        }
    }
}
