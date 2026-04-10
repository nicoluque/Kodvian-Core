using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kodvian.Core.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TaskModuleFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Descripcion",
                table: "Tasks",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreadoPorId",
                table: "Tasks",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateOnly>(
                name: "FechaFinalizacion",
                table: "Tasks",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "FechaInicio",
                table: "Tasks",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "HorasEstimadas",
                table: "Tasks",
                type: "numeric(8,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "HorasReales",
                table: "Tasks",
                type: "numeric(8,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrdenKanban",
                table: "Tasks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_CreadoPorId",
                table: "Tasks",
                column: "CreadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_Estado",
                table: "Tasks",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_FechaVencimiento",
                table: "Tasks",
                column: "FechaVencimiento");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_Prioridad",
                table: "Tasks",
                column: "Prioridad");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Users_CreadoPorId",
                table: "Tasks",
                column: "CreadoPorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Users_CreadoPorId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_CreadoPorId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_Estado",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_FechaVencimiento",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_Prioridad",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "CreadoPorId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "FechaFinalizacion",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "FechaInicio",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "HorasEstimadas",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "HorasReales",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "OrdenKanban",
                table: "Tasks");

            migrationBuilder.AlterColumn<string>(
                name: "Descripcion",
                table: "Tasks",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000,
                oldNullable: true);
        }
    }
}
