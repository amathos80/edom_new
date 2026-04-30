using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eDom.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveFunzioneShadowNavigations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DO $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM information_schema.table_constraints
        WHERE table_schema = 'HICT'
          AND table_name = 'SI_FUNZIONI'
          AND constraint_name = 'FK_SI_FUNZIONI_SI_UTENTI_UtenteInserimentoNavigationId'
    ) THEN
        ALTER TABLE ""HICT"".""SI_FUNZIONI""
        DROP CONSTRAINT ""FK_SI_FUNZIONI_SI_UTENTI_UtenteInserimentoNavigationId"";
    END IF;

    IF EXISTS (
        SELECT 1
        FROM information_schema.table_constraints
        WHERE table_schema = 'HICT'
          AND table_name = 'SI_FUNZIONI'
          AND constraint_name = 'FK_SI_FUNZIONI_SI_UTENTI_UtenteModificaNavigationId'
    ) THEN
        ALTER TABLE ""HICT"".""SI_FUNZIONI""
        DROP CONSTRAINT ""FK_SI_FUNZIONI_SI_UTENTI_UtenteModificaNavigationId"";
    END IF;
END $$;

DROP INDEX IF EXISTS ""HICT"".""IX_SI_FUNZIONI_UtenteInserimentoNavigationId"";
DROP INDEX IF EXISTS ""HICT"".""IX_SI_FUNZIONI_UtenteModificaNavigationId"";

ALTER TABLE ""HICT"".""SI_FUNZIONI"" DROP COLUMN IF EXISTS ""UtenteInserimentoNavigationId"";
ALTER TABLE ""HICT"".""SI_FUNZIONI"" DROP COLUMN IF EXISTS ""UtenteModificaNavigationId"";
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UtenteInserimentoNavigationId",
                schema: "HICT",
                table: "SI_FUNZIONI",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UtenteModificaNavigationId",
                schema: "HICT",
                table: "SI_FUNZIONI",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SI_FUNZIONI_UtenteInserimentoNavigationId",
                schema: "HICT",
                table: "SI_FUNZIONI",
                column: "UtenteInserimentoNavigationId");

            migrationBuilder.CreateIndex(
                name: "IX_SI_FUNZIONI_UtenteModificaNavigationId",
                schema: "HICT",
                table: "SI_FUNZIONI",
                column: "UtenteModificaNavigationId");

            migrationBuilder.AddForeignKey(
                name: "FK_SI_FUNZIONI_SI_UTENTI_UtenteInserimentoNavigationId",
                schema: "HICT",
                table: "SI_FUNZIONI",
                column: "UtenteInserimentoNavigationId",
                principalSchema: "HICT",
                principalTable: "SI_UTENTI",
                principalColumn: "UTEN_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_SI_FUNZIONI_SI_UTENTI_UtenteModificaNavigationId",
                schema: "HICT",
                table: "SI_FUNZIONI",
                column: "UtenteModificaNavigationId",
                principalSchema: "HICT",
                principalTable: "SI_UTENTI",
                principalColumn: "UTEN_ID");
        }
    }
}
