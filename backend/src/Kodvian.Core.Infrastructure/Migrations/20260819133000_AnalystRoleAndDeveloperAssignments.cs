using System;
using Kodvian.Core.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kodvian.Core.Infrastructure.Migrations
{
    [DbContext(typeof(KodvianDbContext))]
    [Migration("20260819133000_AnalystRoleAndDeveloperAssignments")]
    public partial class AnalystRoleAndDeveloperAssignments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
INSERT INTO ""Roles"" (""Id"", ""Name"", ""Description"", ""FechaCreacion"", ""FechaActualizacion"", ""Activo"")
VALUES ('7bbad1f2-2c26-4dc0-bf74-2213871f7d52', 'Analista', 'Gestion operativa de clientes, equipo, proyectos y tareas', '2026-04-01 00:00:00+00', NULL, TRUE)
ON CONFLICT (""Name"") DO NOTHING;");

            migrationBuilder.Sql(@"
CREATE TABLE IF NOT EXISTS ""ProjectDeveloperAssignments"" (
    ""Id"" uuid NOT NULL,
    ""ProjectId"" uuid NOT NULL,
    ""DeveloperId"" uuid NOT NULL,
    ""Notes"" character varying(1000) NULL,
    ""FechaCreacion"" timestamp with time zone NOT NULL,
    ""FechaActualizacion"" timestamp with time zone NULL,
    ""Activo"" boolean NOT NULL,
    CONSTRAINT ""PK_ProjectDeveloperAssignments"" PRIMARY KEY (""Id""),
    CONSTRAINT ""FK_ProjectDeveloperAssignments_Developers_DeveloperId"" FOREIGN KEY (""DeveloperId"") REFERENCES ""Developers"" (""Id"") ON DELETE RESTRICT,
    CONSTRAINT ""FK_ProjectDeveloperAssignments_Projects_ProjectId"" FOREIGN KEY (""ProjectId"") REFERENCES ""Projects"" (""Id"") ON DELETE RESTRICT
);");

            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS \"IX_ProjectDeveloperAssignments_DeveloperId\" ON \"ProjectDeveloperAssignments\" (\"DeveloperId\");");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS \"IX_ProjectDeveloperAssignments_ProjectId\" ON \"ProjectDeveloperAssignments\" (\"ProjectId\");");
            migrationBuilder.Sql("CREATE UNIQUE INDEX IF NOT EXISTS \"IX_ProjectDeveloperAssignments_ProjectId_DeveloperId\" ON \"ProjectDeveloperAssignments\" (\"ProjectId\", \"DeveloperId\");");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE IF EXISTS \"ProjectDeveloperAssignments\";");
            migrationBuilder.DeleteData(table: "Roles", keyColumn: "Id", keyValue: new Guid("7bbad1f2-2c26-4dc0-bf74-2213871f7d52"));
        }
    }
}
