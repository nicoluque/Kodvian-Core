using Kodvian.Core.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kodvian.Core.Infrastructure.Migrations
{
    [DbContext(typeof(KodvianDbContext))]
    [Migration("20260819143000_BackfillAnalystDeveloperProfiles")]
    public partial class BackfillAnalystDeveloperProfiles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE EXTENSION IF NOT EXISTS pgcrypto;

WITH analysts AS (
    SELECT u.""Id"", u.""FullName"", u.""Email"", u.""Activo""
    FROM ""Users"" u
    INNER JOIN ""Roles"" r ON r.""Id"" = u.""RoleId""
    WHERE r.""Name"" = 'Analista'
      AND u.""DeveloperId"" IS NULL
), created_developers AS (
    INSERT INTO ""Developers"" (""Id"", ""FullName"", ""Email"", ""Phone"", ""TaxId"", ""Notes"", ""FechaCreacion"", ""FechaActualizacion"", ""Activo"")
    SELECT gen_random_uuid(), a.""FullName"", a.""Email"", NULL, NULL, 'Perfil remunerable de analista', NOW(), NULL, a.""Activo""
    FROM analysts a
    RETURNING ""Id"", ""Email""
)
UPDATE ""Users"" u
SET ""DeveloperId"" = d.""Id"", ""FechaActualizacion"" = NOW()
FROM created_developers d
WHERE u.""Email"" = d.""Email"";");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
WITH analyst_developers AS (
    SELECT u.""DeveloperId"" AS ""Id""
    FROM ""Users"" u
    INNER JOIN ""Roles"" r ON r.""Id"" = u.""RoleId""
    WHERE r.""Name"" = 'Analista'
      AND u.""DeveloperId"" IS NOT NULL
      AND EXISTS (
          SELECT 1 FROM ""Developers"" d
          WHERE d.""Id"" = u.""DeveloperId""
            AND d.""Notes"" = 'Perfil remunerable de analista'
      )
), cleared_users AS (
    UPDATE ""Users"" u
    SET ""DeveloperId"" = NULL, ""FechaActualizacion"" = NOW()
    FROM analyst_developers ad
    WHERE u.""DeveloperId"" = ad.""Id""
    RETURNING ad.""Id""
)
DELETE FROM ""Developers"" d
USING cleared_users cu
WHERE d.""Id"" = cu.""Id"";");
        }
    }
}
