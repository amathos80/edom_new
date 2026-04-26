using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace eDom.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "HICT");

            migrationBuilder.CreateTable(
                name: "CO_PAZIENTI",
                schema: "HICT",
                columns: table => new
                {
                    PAZI_ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PAZI_CODICE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PAZI_VALID_FROM = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PAZI_VALID_TO = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PAZI_COGNOME = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PAZI_NOME = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PAZI_DTNAS = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PAZI_CODFISC = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PAZI_SESSO = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    PAZI_EMAIL = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PAZI_CODICESANIT = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PAZI_CITTADIN_ID = table.Column<int>(type: "integer", nullable: false),
                    PAZI_COMNAS_ID = table.Column<int>(type: "integer", nullable: false),
                    PAZI_COMRES_ID = table.Column<int>(type: "integer", nullable: false),
                    PAZI_CAPRES = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    PAZI_INDRES = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PAZI_AREARES_ID = table.Column<int>(type: "integer", nullable: false),
                    PAZI_COMDOM_ID = table.Column<int>(type: "integer", nullable: true),
                    PAZI_CAPDOM = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    PAZI_INDDOM = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PAZI_AREADOM_ID = table.Column<int>(type: "integer", nullable: true),
                    PAZI_COMREP_ID = table.Column<int>(type: "integer", nullable: true),
                    PAZI_CAPREP = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    PAZI_INDREP = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PAZI_CAMPREP = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PAZI_AREAREP_ID = table.Column<int>(type: "integer", nullable: true),
                    PAZI_TELEF01 = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    PAZI_TELEF02 = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    PAZI_TELEF03 = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    PAZI_ELTG_STRANDOCTIPO = table.Column<int>(type: "integer", nullable: true),
                    PAZI_STRANDOCNUM = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PAZI_STRANDOCSCAD = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PAZI_TEAMNUM = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PAZI_TEAMSCAD = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PAZI_ESENZ_ID = table.Column<int>(type: "integer", nullable: true),
                    PAZI_DTESENZ = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PAZI_ELTG_STATOCIV = table.Column<int>(type: "integer", nullable: false),
                    PAZI_ELTG_TITSTU = table.Column<int>(type: "integer", nullable: true),
                    PAZI_ELTG_RELIG = table.Column<int>(type: "integer", nullable: true),
                    PAZI_ELTG_EVEANAG = table.Column<int>(type: "integer", nullable: true),
                    PAZI_DTEVEANAG = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PAZI_ELTG_PROFES = table.Column<int>(type: "integer", nullable: true),
                    PAZI_ELTG_CONDPROFES = table.Column<int>(type: "integer", nullable: true),
                    PAZI_ELTG_POSPROFES = table.Column<int>(type: "integer", nullable: true),
                    PAZI_MEDICO_ID = table.Column<int>(type: "integer", nullable: true),
                    PAZI_CONSULTORIO_ID = table.Column<int>(type: "integer", nullable: true),
                    PAZI_COGNOMEPADRE = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PAZI_NOMEPADRE = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PAZI_CODFISCPADRE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    PAZI_DTNASPADRE = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PAZI_INDRESPADRE = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PAZI_COMRESPADRE_ID = table.Column<int>(type: "integer", nullable: true),
                    PAZI_TELEFPADRE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    PAZI_COGNOMEMADRE = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PAZI_NOMEMADRE = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PAZI_CODFISCMADRE = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    PAZI_DTNASMADRE = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PAZI_INDRESMADRE = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PAZI_COMRESMADRE_ID = table.Column<int>(type: "integer", nullable: true),
                    PAZI_TELEFMADRE = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    PAZI_COGNOMEFAM1 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PAZI_NOMEFAM1 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PAZI_CODFISCFAM1 = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    PAZI_DTNASFAM1 = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PAZI_INDRESFAM1 = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PAZI_COMRESFAM1_ID = table.Column<int>(type: "integer", nullable: true),
                    PAZI_TELEFFAM1 = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    PAZI_CODALT01 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PAZI_CODALT02 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PAZI_NOTE01 = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    PAZI_UTINS = table.Column<int>(type: "integer", nullable: false),
                    PAZI_DTINS = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PAZI_UTMOD = table.Column<int>(type: "integer", nullable: true),
                    PAZI_DTMOD = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PAZI_VERSION = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PAZI_F_ATT = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CO_PAZIENTI", x => x.PAZI_ID);
                });

            migrationBuilder.CreateTable(
                name: "SI_AUDIT_LOG",
                schema: "HICT",
                columns: table => new
                {
                    AULO_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AULO_TABELLA = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AULO_ENTITA_ID = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AULO_OPERAZIONE = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    AULO_UTEN_ID = table.Column<int>(type: "integer", nullable: true),
                    AULO_DTOP = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AULO_OLD_VALUES = table.Column<string>(type: "text", nullable: true),
                    AULO_NEW_VALUES = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SI_AUDIT_LOG", x => x.AULO_ID);
                });

            migrationBuilder.CreateTable(
                name: "SI_CONFIG",
                schema: "HICT",
                columns: table => new
                {
                    CONF_ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CONF_PROC_ID = table.Column<int>(type: "integer", nullable: false),
                    CONF_CODICE = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CONF_VALORE = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CONF_DESCR = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SI_CONFIG", x => x.CONF_ID);
                });

            migrationBuilder.CreateTable(
                name: "SI_LOGACC",
                schema: "HICT",
                columns: table => new
                {
                    LOAC_ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LOAC_UTEN_ID = table.Column<int>(type: "integer", nullable: false),
                    LOAC_DATE = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LOAC_IPADDR = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LOAC_MACHINE = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    LOAC_PROC_ID = table.Column<int>(type: "integer", nullable: false),
                    LOAC_FUNZ_ID = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SI_LOGACC", x => x.LOAC_ID);
                });

            migrationBuilder.CreateTable(
                name: "SI_RUOLI",
                schema: "HICT",
                columns: table => new
                {
                    RUOL_ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RUOL_PROC_ID = table.Column<int>(type: "integer", nullable: false),
                    RUOL_CODICE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RUOL_DESCR = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    RUOL_F_ADMIN = table.Column<short>(type: "smallint", nullable: false),
                    RUOL_UTINS = table.Column<int>(type: "integer", nullable: false),
                    RUOL_DTINS = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RUOL_UTMOD = table.Column<int>(type: "integer", nullable: true),
                    RUOL_DTMOD = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RUOL_VERSION = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SI_RUOLI", x => x.RUOL_ID);
                });

            migrationBuilder.CreateTable(
                name: "SI_UTENTI",
                schema: "HICT",
                columns: table => new
                {
                    UTEN_ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UTEN_CODICE = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UTEN_PASSWD = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    UTEN_COGNOME = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UTEN_NOME = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UTEN_CODFISC = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    UTEN_EMAIL = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    UTEN_MATR = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    UTEN_F_CHPASS = table.Column<short>(type: "smallint", nullable: false),
                    UTEN_F_SMCARD = table.Column<short>(type: "smallint", nullable: false),
                    UTEN_DTDISAT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UTEN_DTRIAT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UTEN_DTSCPASS = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UTEN_LASTLOGIN = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UTEN_UTINS = table.Column<int>(type: "integer", nullable: false),
                    UTEN_DTINS = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UTEN_UTMOD = table.Column<int>(type: "integer", nullable: true),
                    UTEN_DTMOD = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UTEN_VERSION = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SI_UTENTI", x => x.UTEN_ID);
                });

            migrationBuilder.CreateTable(
                name: "SI_UTENRUOL",
                schema: "HICT",
                columns: table => new
                {
                    RuoliId = table.Column<int>(type: "integer", nullable: false),
                    UtentiId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SI_UTENRUOL", x => new { x.RuoliId, x.UtentiId });
                    table.ForeignKey(
                        name: "FK_SI_UTENRUOL_SI_RUOLI_RuoliId",
                        column: x => x.RuoliId,
                        principalSchema: "HICT",
                        principalTable: "SI_RUOLI",
                        principalColumn: "RUOL_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SI_UTENRUOL_SI_UTENTI_UtentiId",
                        column: x => x.UtentiId,
                        principalSchema: "HICT",
                        principalTable: "SI_UTENTI",
                        principalColumn: "UTEN_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SI_UTENRUOL_UtentiId",
                schema: "HICT",
                table: "SI_UTENRUOL",
                column: "UtentiId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CO_PAZIENTI",
                schema: "HICT");

            migrationBuilder.DropTable(
                name: "SI_AUDIT_LOG",
                schema: "HICT");

            migrationBuilder.DropTable(
                name: "SI_CONFIG",
                schema: "HICT");

            migrationBuilder.DropTable(
                name: "SI_LOGACC",
                schema: "HICT");

            migrationBuilder.DropTable(
                name: "SI_UTENRUOL",
                schema: "HICT");

            migrationBuilder.DropTable(
                name: "SI_RUOLI",
                schema: "HICT");

            migrationBuilder.DropTable(
                name: "SI_UTENTI",
                schema: "HICT");
        }
    }
}
