using System;
using Kodvian.Core.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kodvian.Core.Infrastructure.Migrations
{
    [DbContext(typeof(KodvianDbContext))]
    [Migration("20260825210000_GitHubIntegrationSchema")]
    public partial class GitHubIntegrationSchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
ALTER TABLE ""Projects"" ADD COLUMN IF NOT EXISTS ""GitHubOwner"" character varying(100) NULL;
ALTER TABLE ""Projects"" ADD COLUMN IF NOT EXISTS ""GitHubRepoName"" character varying(200) NULL;
ALTER TABLE ""Projects"" ADD COLUMN IF NOT EXISTS ""GitHubRepoId"" bigint NULL;
ALTER TABLE ""Projects"" ADD COLUMN IF NOT EXISTS ""GitHubRepoUrl"" character varying(500) NULL;");

            migrationBuilder.Sql(@"
CREATE UNIQUE INDEX IF NOT EXISTS ""IX_Projects_GitHubOwner_GitHubRepoName""
ON ""Projects"" (""GitHubOwner"", ""GitHubRepoName"")
WHERE ""GitHubOwner"" IS NOT NULL AND ""GitHubRepoName"" IS NOT NULL;");

            migrationBuilder.Sql(@"
ALTER TABLE ""Users"" ADD COLUMN IF NOT EXISTS ""GitHubUsername"" character varying(100) NULL;
ALTER TABLE ""Users"" ADD COLUMN IF NOT EXISTS ""GitHubUserId"" bigint NULL;
ALTER TABLE ""Users"" ADD COLUMN IF NOT EXISTS ""GitHubAccessTokenEncrypted"" character varying(4000) NULL;
ALTER TABLE ""Users"" ADD COLUMN IF NOT EXISTS ""GitHubConnectedAt"" timestamp with time zone NULL;");

            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS \"IX_Users_GitHubUsername\" ON \"Users\" (\"GitHubUsername\");");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS \"IX_Users_GitHubUserId\" ON \"Users\" (\"GitHubUserId\");");

            migrationBuilder.Sql(@"
CREATE TABLE IF NOT EXISTS ""GitHubIssueLinks"" (
    ""Id"" uuid NOT NULL,
    ""ProjectId"" uuid NOT NULL,
    ""DeveloperId"" uuid NOT NULL,
    ""GitHubIssueNumber"" integer NOT NULL,
    ""GitHubIssueNodeId"" character varying(80) NOT NULL,
    ""GitHubIssueUrl"" character varying(500) NOT NULL,
    ""Title"" character varying(500) NOT NULL,
    ""Description"" character varying(8000) NULL,
    ""Status"" integer NOT NULL,
    ""Priority"" integer NULL,
    ""AssignedGitHubUsername"" character varying(100) NULL,
    ""LastSyncedAt"" timestamp with time zone NULL,
    ""SyncDirection"" integer NOT NULL,
    ""FechaCreacion"" timestamp with time zone NOT NULL,
    ""FechaActualizacion"" timestamp with time zone NULL,
    ""Activo"" boolean NOT NULL,
    CONSTRAINT ""PK_GitHubIssueLinks"" PRIMARY KEY (""Id""),
    CONSTRAINT ""FK_GitHubIssueLinks_Projects_ProjectId"" FOREIGN KEY (""ProjectId"") REFERENCES ""Projects"" (""Id"") ON DELETE RESTRICT,
    CONSTRAINT ""FK_GitHubIssueLinks_Developers_DeveloperId"" FOREIGN KEY (""DeveloperId"") REFERENCES ""Developers"" (""Id"") ON DELETE RESTRICT
);");

            migrationBuilder.Sql("CREATE UNIQUE INDEX IF NOT EXISTS \"IX_GitHubIssueLinks_ProjectId_GitHubIssueNumber\" ON \"GitHubIssueLinks\" (\"ProjectId\", \"GitHubIssueNumber\");");
            migrationBuilder.Sql("CREATE UNIQUE INDEX IF NOT EXISTS \"IX_GitHubIssueLinks_GitHubIssueNodeId\" ON \"GitHubIssueLinks\" (\"GitHubIssueNodeId\");");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS \"IX_GitHubIssueLinks_DeveloperId_Status\" ON \"GitHubIssueLinks\" (\"DeveloperId\", \"Status\");");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS \"IX_GitHubIssueLinks_ProjectId\" ON \"GitHubIssueLinks\" (\"ProjectId\");");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS \"IX_GitHubIssueLinks_LastSyncedAt\" ON \"GitHubIssueLinks\" (\"LastSyncedAt\");");

            migrationBuilder.Sql(@"
CREATE TABLE IF NOT EXISTS ""GitHubOAuthStates"" (
    ""Id"" uuid NOT NULL,
    ""StateToken"" character varying(128) NOT NULL,
    ""UserId"" uuid NOT NULL,
    ""ExpiresAt"" timestamp with time zone NOT NULL,
    ""CreatedAt"" timestamp with time zone NOT NULL,
    CONSTRAINT ""PK_GitHubOAuthStates"" PRIMARY KEY (""Id""),
    CONSTRAINT ""FK_GitHubOAuthStates_Users_UserId"" FOREIGN KEY (""UserId"") REFERENCES ""Users"" (""Id"") ON DELETE CASCADE
);");

            migrationBuilder.Sql("CREATE UNIQUE INDEX IF NOT EXISTS \"IX_GitHubOAuthStates_StateToken\" ON \"GitHubOAuthStates\" (\"StateToken\");");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS \"IX_GitHubOAuthStates_ExpiresAt\" ON \"GitHubOAuthStates\" (\"ExpiresAt\");");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS \"IX_GitHubOAuthStates_UserId\" ON \"GitHubOAuthStates\" (\"UserId\");");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE IF EXISTS \"GitHubOAuthStates\";");
            migrationBuilder.Sql("DROP TABLE IF EXISTS \"GitHubIssueLinks\";");

            migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_Users_GitHubUserId\";");
            migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_Users_GitHubUsername\";");
            migrationBuilder.Sql("ALTER TABLE \"Users\" DROP COLUMN IF EXISTS \"GitHubConnectedAt\";");
            migrationBuilder.Sql("ALTER TABLE \"Users\" DROP COLUMN IF EXISTS \"GitHubAccessTokenEncrypted\";");
            migrationBuilder.Sql("ALTER TABLE \"Users\" DROP COLUMN IF EXISTS \"GitHubUserId\";");
            migrationBuilder.Sql("ALTER TABLE \"Users\" DROP COLUMN IF EXISTS \"GitHubUsername\";");

            migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_Projects_GitHubOwner_GitHubRepoName\";");
            migrationBuilder.Sql("ALTER TABLE \"Projects\" DROP COLUMN IF EXISTS \"GitHubRepoUrl\";");
            migrationBuilder.Sql("ALTER TABLE \"Projects\" DROP COLUMN IF EXISTS \"GitHubRepoId\";");
            migrationBuilder.Sql("ALTER TABLE \"Projects\" DROP COLUMN IF EXISTS \"GitHubRepoName\";");
            migrationBuilder.Sql("ALTER TABLE \"Projects\" DROP COLUMN IF EXISTS \"GitHubOwner\";");
        }
    }
}
