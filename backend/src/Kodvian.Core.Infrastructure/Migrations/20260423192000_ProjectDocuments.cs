using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kodvian.Core.Infrastructure.Migrations
{
    public partial class ProjectDocuments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_DocumentFiles_Owner",
                table: "DocumentFiles");

            migrationBuilder.AddColumn<Guid>(
                name: "ProjectId",
                table: "DocumentFiles",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentFiles_ProjectId",
                table: "DocumentFiles",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentFiles_Projects_ProjectId",
                table: "DocumentFiles",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddCheckConstraint(
                name: "CK_DocumentFiles_Owner",
                table: "DocumentFiles",
                sql: "((\"ProjectId\" IS NOT NULL)::int + (\"FinancialMovementId\" IS NOT NULL)::int + (\"DeveloperPaymentId\" IS NOT NULL)::int) = 1");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_DocumentFiles_Owner",
                table: "DocumentFiles");

            migrationBuilder.DropForeignKey(
                name: "FK_DocumentFiles_Projects_ProjectId",
                table: "DocumentFiles");

            migrationBuilder.DropIndex(
                name: "IX_DocumentFiles_ProjectId",
                table: "DocumentFiles");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "DocumentFiles");

            migrationBuilder.AddCheckConstraint(
                name: "CK_DocumentFiles_Owner",
                table: "DocumentFiles",
                sql: "(\"FinancialMovementId\" IS NOT NULL AND \"DeveloperPaymentId\" IS NULL) OR (\"FinancialMovementId\" IS NULL AND \"DeveloperPaymentId\" IS NOT NULL)");
        }
    }
}
