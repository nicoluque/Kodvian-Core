using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kodvian.Core.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ClientModuleFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InformacionComercial",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "Telefono",
                table: "Clients");

            migrationBuilder.RenameColumn(
                name: "Nombre",
                table: "Clients",
                newName: "CommercialName");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "Clients",
                newName: "ServiceType");

            migrationBuilder.RenameColumn(
                name: "Contacto",
                table: "Clients",
                newName: "Province");

            migrationBuilder.RenameIndex(
                name: "IX_Clients_Nombre",
                table: "Clients",
                newName: "IX_Clients_CommercialName");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Clients",
                type: "character varying(180)",
                maxLength: 180,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BillingDay",
                table: "Clients",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Clients",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactEmail",
                table: "Clients",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactName",
                table: "Clients",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactPhone",
                table: "Clients",
                type: "character varying(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "Clients",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LegalName",
                table: "Clients",
                type: "character varying(220)",
                maxLength: 220,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlyAmount",
                table: "Clients",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Clients",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Clients",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "TaxId",
                table: "Clients",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("24b2ab35-0c84-4fa8-9c35-eecf5f476bb8"),
                column: "FechaCreacion",
                value: new DateTime(2026, 4, 7, 23, 2, 58, 947, DateTimeKind.Utc).AddTicks(139));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("95e52dc4-44f5-4b0b-aabf-4044f28cc55a"),
                column: "FechaCreacion",
                value: new DateTime(2026, 4, 7, 23, 2, 58, 947, DateTimeKind.Utc).AddTicks(133));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("a77176f8-d33a-4c23-b613-f2e73093e1b7"),
                column: "FechaCreacion",
                value: new DateTime(2026, 4, 7, 23, 2, 58, 947, DateTimeKind.Utc).AddTicks(137));

            migrationBuilder.CreateIndex(
                name: "IX_Clients_Status",
                table: "Clients",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Clients_Status",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "BillingDay",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "ContactEmail",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "ContactName",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "ContactPhone",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "LegalName",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "MonthlyAmount",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "TaxId",
                table: "Clients");

            migrationBuilder.RenameColumn(
                name: "ServiceType",
                table: "Clients",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "Province",
                table: "Clients",
                newName: "Contacto");

            migrationBuilder.RenameColumn(
                name: "CommercialName",
                table: "Clients",
                newName: "Nombre");

            migrationBuilder.RenameIndex(
                name: "IX_Clients_CommercialName",
                table: "Clients",
                newName: "IX_Clients_Nombre");

            migrationBuilder.AddColumn<string>(
                name: "InformacionComercial",
                table: "Clients",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Telefono",
                table: "Clients",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("24b2ab35-0c84-4fa8-9c35-eecf5f476bb8"),
                column: "FechaCreacion",
                value: new DateTime(2026, 4, 7, 22, 34, 8, 814, DateTimeKind.Utc).AddTicks(6032));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("95e52dc4-44f5-4b0b-aabf-4044f28cc55a"),
                column: "FechaCreacion",
                value: new DateTime(2026, 4, 7, 22, 34, 8, 814, DateTimeKind.Utc).AddTicks(6017));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("a77176f8-d33a-4c23-b613-f2e73093e1b7"),
                column: "FechaCreacion",
                value: new DateTime(2026, 4, 7, 22, 34, 8, 814, DateTimeKind.Utc).AddTicks(6028));
        }
    }
}
