using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kodvian.Core.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ProjectDocumentsPhase2Versioning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DO $$
                BEGIN
                    ALTER TABLE "DocumentFiles" DROP CONSTRAINT IF EXISTS "CK_DocumentFiles_Owner";

                    IF NOT EXISTS (
                        SELECT 1
                        FROM information_schema.columns
                        WHERE table_schema = 'public'
                          AND table_name = 'DocumentFiles'
                          AND column_name = 'ProjectId'
                    ) THEN
                        ALTER TABLE "DocumentFiles" ADD COLUMN "ProjectId" uuid NULL;
                    END IF;

                    IF NOT EXISTS (
                        SELECT 1
                        FROM pg_indexes
                        WHERE schemaname = 'public'
                          AND tablename = 'DocumentFiles'
                          AND indexname = 'IX_DocumentFiles_ProjectId'
                    ) THEN
                        CREATE INDEX "IX_DocumentFiles_ProjectId" ON "DocumentFiles" ("ProjectId");
                    END IF;

                    IF NOT EXISTS (
                        SELECT 1
                        FROM pg_constraint
                        WHERE conname = 'FK_DocumentFiles_Projects_ProjectId'
                    ) THEN
                        ALTER TABLE "DocumentFiles"
                            ADD CONSTRAINT "FK_DocumentFiles_Projects_ProjectId"
                            FOREIGN KEY ("ProjectId")
                            REFERENCES "Projects" ("Id")
                            ON DELETE SET NULL;
                    END IF;

                    ALTER TABLE "DocumentFiles"
                        ADD CONSTRAINT "CK_DocumentFiles_Owner"
                        CHECK ((("ProjectId" IS NOT NULL)::int + ("FinancialMovementId" IS NOT NULL)::int + ("DeveloperPaymentId" IS NOT NULL)::int) = 1);
                END $$;
                """);

            migrationBuilder.CreateTable(
                name: "ProjectDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    DeletedById = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectDocuments_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectDocuments_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectDocuments_Users_DeletedById",
                        column: x => x.DeletedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ProjectDocumentVersions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectDocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentFileId = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    UploadedById = table.Column<Guid>(type: "uuid", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectDocumentVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectDocumentVersions_DocumentFiles_DocumentFileId",
                        column: x => x.DocumentFileId,
                        principalTable: "DocumentFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectDocumentVersions_ProjectDocuments_ProjectDocumentId",
                        column: x => x.ProjectDocumentId,
                        principalTable: "ProjectDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectDocumentVersions_Users_UploadedById",
                        column: x => x.UploadedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectDocuments_CreatedById",
                table: "ProjectDocuments",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectDocuments_DeletedById",
                table: "ProjectDocuments",
                column: "DeletedById");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectDocuments_ProjectId",
                table: "ProjectDocuments",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectDocuments_ProjectId_Activo",
                table: "ProjectDocuments",
                columns: new[] { "ProjectId", "Activo" });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectDocuments_Type",
                table: "ProjectDocuments",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectDocumentVersions_DocumentFileId",
                table: "ProjectDocumentVersions",
                column: "DocumentFileId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectDocumentVersions_ProjectDocumentId",
                table: "ProjectDocumentVersions",
                column: "ProjectDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectDocumentVersions_ProjectDocumentId_VersionNumber",
                table: "ProjectDocumentVersions",
                columns: new[] { "ProjectDocumentId", "VersionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectDocumentVersions_UploadedById",
                table: "ProjectDocumentVersions",
                column: "UploadedById");

            migrationBuilder.Sql(
                """
                INSERT INTO "ProjectDocuments" ("Id", "ProjectId", "Type", "Title", "CreatedById", "DeletedById", "DeletedAt", "FechaCreacion", "FechaActualizacion", "Activo")
                SELECT d."Id",
                       d."ProjectId",
                       7,
                       LEFT(CASE
                                WHEN btrim(COALESCE(d."OriginalFileName", '')) = '' THEN 'Documento'
                                ELSE d."OriginalFileName"
                            END, 200),
                       d."UploadedById",
                       NULL,
                       NULL,
                       d."FechaCreacion",
                       NULL,
                       TRUE
                FROM "DocumentFiles" d
                WHERE d."ProjectId" IS NOT NULL
                  AND NOT EXISTS (
                    SELECT 1
                    FROM "ProjectDocuments" pd
                    WHERE pd."Id" = d."Id"
                );

                INSERT INTO "ProjectDocumentVersions" ("Id", "ProjectDocumentId", "DocumentFileId", "VersionNumber", "Notes", "UploadedById", "FechaCreacion", "FechaActualizacion", "Activo")
                SELECT d."Id",
                       d."Id",
                       d."Id",
                       1,
                       NULL,
                       d."UploadedById",
                       d."FechaCreacion",
                       NULL,
                       TRUE
                FROM "DocumentFiles" d
                WHERE d."ProjectId" IS NOT NULL
                  AND EXISTS (
                    SELECT 1
                    FROM "ProjectDocuments" pd
                    WHERE pd."Id" = d."Id"
                )
                  AND NOT EXISTS (
                    SELECT 1
                    FROM "ProjectDocumentVersions" pv
                    WHERE pv."Id" = d."Id"
                );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectDocumentVersions");

            migrationBuilder.DropTable(
                name: "ProjectDocuments");

        }
    }
}
