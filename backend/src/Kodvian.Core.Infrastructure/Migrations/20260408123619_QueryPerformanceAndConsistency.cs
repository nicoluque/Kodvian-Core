using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kodvian.Core.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class QueryPerformanceAndConsistency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Tasks_Estado_OrdenKanban_FechaCreacion",
                table: "Tasks",
                columns: new[] { "Estado", "OrdenKanban", "FechaCreacion" });

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_ProyectoId_Estado",
                table: "Tasks",
                columns: new[] { "ProyectoId", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_Projects_ClienteId_FechaCreacion",
                table: "Projects",
                columns: new[] { "ClienteId", "FechaCreacion" });

            migrationBuilder.CreateIndex(
                name: "IX_Projects_Estado_Prioridad_FechaCreacion",
                table: "Projects",
                columns: new[] { "Estado", "Prioridad", "FechaCreacion" });

            migrationBuilder.CreateIndex(
                name: "IX_FinancialMovements_MovementDate_FechaCreacion",
                table: "FinancialMovements",
                columns: new[] { "MovementDate", "FechaCreacion" });

            migrationBuilder.CreateIndex(
                name: "IX_FinancialMovements_MovementType_Status_DueDate",
                table: "FinancialMovements",
                columns: new[] { "MovementType", "Status", "DueDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tasks_Estado_OrdenKanban_FechaCreacion",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_ProyectoId_Estado",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Projects_ClienteId_FechaCreacion",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_Estado_Prioridad_FechaCreacion",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_FinancialMovements_MovementDate_FechaCreacion",
                table: "FinancialMovements");

            migrationBuilder.DropIndex(
                name: "IX_FinancialMovements_MovementType_Status_DueDate",
                table: "FinancialMovements");
        }
    }
}
