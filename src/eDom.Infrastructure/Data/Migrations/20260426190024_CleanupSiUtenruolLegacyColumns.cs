using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eDom.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class CleanupSiUtenruolLegacyColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1
                        FROM information_schema.columns
                        WHERE table_schema = 'HICT' AND table_name = 'SI_UTENRUOL' AND column_name = 'UTRU_PROC_ID' AND udt_name <> 'int4'
                    ) THEN
                        ALTER TABLE "HICT"."SI_UTENRUOL"
                        ALTER COLUMN "UTRU_PROC_ID" TYPE integer USING "UTRU_PROC_ID"::integer;
                    END IF;

                    IF EXISTS (
                        SELECT 1
                        FROM information_schema.columns
                        WHERE table_schema = 'HICT' AND table_name = 'SI_UTENRUOL' AND column_name = 'RuoliId'
                    ) THEN
                        ALTER TABLE "HICT"."SI_UTENRUOL" DROP COLUMN "RuoliId";
                    END IF;

                    IF EXISTS (
                        SELECT 1
                        FROM information_schema.columns
                        WHERE table_schema = 'HICT' AND table_name = 'SI_UTENRUOL' AND column_name = 'UtentiId'
                    ) THEN
                        ALTER TABLE "HICT"."SI_UTENRUOL" DROP COLUMN "UtentiId";
                    END IF;
                END $$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1
                        FROM information_schema.columns
                        WHERE table_schema = 'HICT' AND table_name = 'SI_UTENRUOL' AND column_name = 'RuoliId'
                    ) THEN
                        ALTER TABLE "HICT"."SI_UTENRUOL" ADD COLUMN "RuoliId" integer NOT NULL DEFAULT 0;
                    END IF;

                    IF NOT EXISTS (
                        SELECT 1
                        FROM information_schema.columns
                        WHERE table_schema = 'HICT' AND table_name = 'SI_UTENRUOL' AND column_name = 'UtentiId'
                    ) THEN
                        ALTER TABLE "HICT"."SI_UTENRUOL" ADD COLUMN "UtentiId" integer NOT NULL DEFAULT 0;
                    END IF;

                    IF EXISTS (
                        SELECT 1
                        FROM information_schema.columns
                        WHERE table_schema = 'HICT' AND table_name = 'SI_UTENRUOL' AND column_name = 'UTRU_PROC_ID' AND udt_name = 'int4'
                    ) THEN
                        ALTER TABLE "HICT"."SI_UTENRUOL"
                        ALTER COLUMN "UTRU_PROC_ID" TYPE numeric USING "UTRU_PROC_ID"::numeric;
                    END IF;
                END $$;
                """);
        }
    }
}
