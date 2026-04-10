using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kodvian.Core.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FinanceModuleEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Categoria",
                table: "FinancialMovements");

            migrationBuilder.DropColumn(
                name: "Descripcion",
                table: "FinancialMovements");

            migrationBuilder.RenameColumn(
                name: "Tipo",
                table: "FinancialMovements",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "Monto",
                table: "FinancialMovements",
                newName: "Amount");

            migrationBuilder.RenameColumn(
                name: "Fecha",
                table: "FinancialMovements",
                newName: "MovementDate");

            migrationBuilder.RenameIndex(
                name: "IX_FinancialMovements_Fecha",
                table: "FinancialMovements",
                newName: "IX_FinancialMovements_MovementDate");

            migrationBuilder.AddColumn<Guid>(
                name: "CategoryId",
                table: "FinancialMovements",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ClientId",
                table: "FinancialMovements",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedById",
                table: "FinancialMovements",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "FinancialMovements",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateOnly>(
                name: "DueDate",
                table: "FinancialMovements",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MovementType",
                table: "FinancialMovements",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "FinancialMovements",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethod",
                table: "FinancialMovements",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProjectId",
                table: "FinancialMovements",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProviderId",
                table: "FinancialMovements",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReceiptNumber",
                table: "FinancialMovements",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FinancialCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    MovementType = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Providers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    TaxId = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    Phone = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Providers", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FinancialMovements_CategoryId",
                table: "FinancialMovements",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialMovements_ClientId",
                table: "FinancialMovements",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialMovements_CreatedById",
                table: "FinancialMovements",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialMovements_DueDate",
                table: "FinancialMovements",
                column: "DueDate");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialMovements_MovementType",
                table: "FinancialMovements",
                column: "MovementType");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialMovements_ProjectId",
                table: "FinancialMovements",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialMovements_ProviderId",
                table: "FinancialMovements",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialMovements_Status",
                table: "FinancialMovements",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialCategories_MovementType",
                table: "FinancialCategories",
                column: "MovementType");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialCategories_Name",
                table: "FinancialCategories",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Providers_Name",
                table: "Providers",
                column: "Name");

            migrationBuilder.AddForeignKey(
                name: "FK_FinancialMovements_Clients_ClientId",
                table: "FinancialMovements",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_FinancialMovements_FinancialCategories_CategoryId",
                table: "FinancialMovements",
                column: "CategoryId",
                principalTable: "FinancialCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FinancialMovements_Projects_ProjectId",
                table: "FinancialMovements",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_FinancialMovements_Providers_ProviderId",
                table: "FinancialMovements",
                column: "ProviderId",
                principalTable: "Providers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_FinancialMovements_Users_CreatedById",
                table: "FinancialMovements",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FinancialMovements_Clients_ClientId",
                table: "FinancialMovements");

            migrationBuilder.DropForeignKey(
                name: "FK_FinancialMovements_FinancialCategories_CategoryId",
                table: "FinancialMovements");

            migrationBuilder.DropForeignKey(
                name: "FK_FinancialMovements_Projects_ProjectId",
                table: "FinancialMovements");

            migrationBuilder.DropForeignKey(
                name: "FK_FinancialMovements_Providers_ProviderId",
                table: "FinancialMovements");

            migrationBuilder.DropForeignKey(
                name: "FK_FinancialMovements_Users_CreatedById",
                table: "FinancialMovements");

            migrationBuilder.DropTable(
                name: "FinancialCategories");

            migrationBuilder.DropTable(
                name: "Providers");

            migrationBuilder.DropIndex(
                name: "IX_FinancialMovements_CategoryId",
                table: "FinancialMovements");

            migrationBuilder.DropIndex(
                name: "IX_FinancialMovements_ClientId",
                table: "FinancialMovements");

            migrationBuilder.DropIndex(
                name: "IX_FinancialMovements_CreatedById",
                table: "FinancialMovements");

            migrationBuilder.DropIndex(
                name: "IX_FinancialMovements_DueDate",
                table: "FinancialMovements");

            migrationBuilder.DropIndex(
                name: "IX_FinancialMovements_MovementType",
                table: "FinancialMovements");

            migrationBuilder.DropIndex(
                name: "IX_FinancialMovements_ProjectId",
                table: "FinancialMovements");

            migrationBuilder.DropIndex(
                name: "IX_FinancialMovements_ProviderId",
                table: "FinancialMovements");

            migrationBuilder.DropIndex(
                name: "IX_FinancialMovements_Status",
                table: "FinancialMovements");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "FinancialMovements");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "FinancialMovements");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "FinancialMovements");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "FinancialMovements");

            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "FinancialMovements");

            migrationBuilder.DropColumn(
                name: "MovementType",
                table: "FinancialMovements");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "FinancialMovements");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "FinancialMovements");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "FinancialMovements");

            migrationBuilder.DropColumn(
                name: "ProviderId",
                table: "FinancialMovements");

            migrationBuilder.DropColumn(
                name: "ReceiptNumber",
                table: "FinancialMovements");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "FinancialMovements",
                newName: "Tipo");

            migrationBuilder.RenameColumn(
                name: "MovementDate",
                table: "FinancialMovements",
                newName: "Fecha");

            migrationBuilder.RenameColumn(
                name: "Amount",
                table: "FinancialMovements",
                newName: "Monto");

            migrationBuilder.RenameIndex(
                name: "IX_FinancialMovements_MovementDate",
                table: "FinancialMovements",
                newName: "IX_FinancialMovements_Fecha");

            migrationBuilder.AddColumn<string>(
                name: "Categoria",
                table: "FinancialMovements",
                type: "character varying(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Descripcion",
                table: "FinancialMovements",
                type: "text",
                nullable: true);
        }
    }
}
