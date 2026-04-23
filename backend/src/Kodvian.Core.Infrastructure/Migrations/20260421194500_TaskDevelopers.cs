using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kodvian.Core.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TaskDevelopers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DeveloperId",
                table: "Tasks",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_DeveloperId",
                table: "Tasks",
                column: "DeveloperId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Developers_DeveloperId",
                table: "Tasks",
                column: "DeveloperId",
                principalTable: "Developers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Developers_DeveloperId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_DeveloperId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "DeveloperId",
                table: "Tasks");
        }
    }
}
