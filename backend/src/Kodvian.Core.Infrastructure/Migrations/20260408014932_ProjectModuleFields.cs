using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kodvian.Core.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ProjectModuleFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FechaFin",
                table: "Projects",
                newName: "FechaEntregaEstimada");

            migrationBuilder.AddColumn<string>(
                name: "Descripcion",
                table: "Projects",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "FechaCierre",
                table: "Projects",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PorcentajeAvance",
                table: "Projects",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "Presupuesto",
                table: "Projects",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("24b2ab35-0c84-4fa8-9c35-eecf5f476bb8"),
                column: "FechaCreacion",
                value: new DateTime(2026, 4, 8, 1, 49, 30, 529, DateTimeKind.Utc).AddTicks(209));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("95e52dc4-44f5-4b0b-aabf-4044f28cc55a"),
                column: "FechaCreacion",
                value: new DateTime(2026, 4, 8, 1, 49, 30, 529, DateTimeKind.Utc).AddTicks(199));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("a77176f8-d33a-4c23-b613-f2e73093e1b7"),
                column: "FechaCreacion",
                value: new DateTime(2026, 4, 8, 1, 49, 30, 529, DateTimeKind.Utc).AddTicks(207));

            migrationBuilder.CreateIndex(
                name: "IX_Projects_Estado",
                table: "Projects",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_Prioridad",
                table: "Projects",
                column: "Prioridad");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Projects_Estado",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_Prioridad",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "Descripcion",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "FechaCierre",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "PorcentajeAvance",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "Presupuesto",
                table: "Projects");

            migrationBuilder.RenameColumn(
                name: "FechaEntregaEstimada",
                table: "Projects",
                newName: "FechaFin");

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
        }
    }
}
