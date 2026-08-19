using System;
using Kodvian.Core.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kodvian.Core.Infrastructure.Migrations
{
    [DbContext(typeof(KodvianDbContext))]
    [Migration("20260819120000_DeveloperUserAccess")]
    public partial class DeveloperUserAccess : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE \"Users\" ADD COLUMN IF NOT EXISTS \"DeveloperId\" uuid NULL;");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS \"IX_Users_DeveloperId\" ON \"Users\" (\"DeveloperId\");");
            migrationBuilder.Sql(@"
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM pg_constraint
        WHERE conname = 'FK_Users_Developers_DeveloperId') THEN
        ALTER TABLE ""Users""
            ADD CONSTRAINT ""FK_Users_Developers_DeveloperId""
            FOREIGN KEY (""DeveloperId"")
            REFERENCES ""Developers"" (""Id"")
            ON DELETE SET NULL;
    END IF;
END
$$;");

            migrationBuilder.Sql(@"
INSERT INTO ""Roles"" (""Id"", ""Name"", ""Description"", ""FechaCreacion"", ""FechaActualizacion"", ""Activo"")
VALUES ('58df40de-1019-4dcf-81dc-55a8a2d06235', 'Desarrollador', 'Acceso limitado a proyectos y tareas asignadas', '2026-04-01 00:00:00+00', NULL, TRUE)
ON CONFLICT (""Name"") DO NOTHING;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(table: "Roles", keyColumn: "Id", keyValue: new Guid("58df40de-1019-4dcf-81dc-55a8a2d06235"));
            migrationBuilder.Sql("ALTER TABLE \"Users\" DROP CONSTRAINT IF EXISTS \"FK_Users_Developers_DeveloperId\";");
            migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_Users_DeveloperId\";");
            migrationBuilder.Sql("ALTER TABLE \"Users\" DROP COLUMN IF EXISTS \"DeveloperId\";");
        }
    }
}
