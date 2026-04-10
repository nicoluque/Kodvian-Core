using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kodvian.Core.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ClientStatusDefaultCleanup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Clients",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 1);

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("24b2ab35-0c84-4fa8-9c35-eecf5f476bb8"),
                column: "FechaCreacion",
                value: new DateTime(2026, 4, 8, 1, 51, 56, 776, DateTimeKind.Utc).AddTicks(9271));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("95e52dc4-44f5-4b0b-aabf-4044f28cc55a"),
                column: "FechaCreacion",
                value: new DateTime(2026, 4, 8, 1, 51, 56, 776, DateTimeKind.Utc).AddTicks(9262));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("a77176f8-d33a-4c23-b613-f2e73093e1b7"),
                column: "FechaCreacion",
                value: new DateTime(2026, 4, 8, 1, 51, 56, 776, DateTimeKind.Utc).AddTicks(9269));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Clients",
                type: "integer",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "integer");

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
        }
    }
}
