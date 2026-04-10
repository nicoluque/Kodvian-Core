using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kodvian.Core.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ProjectAndRoleSeedStability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("24b2ab35-0c84-4fa8-9c35-eecf5f476bb8"),
                column: "FechaCreacion",
                value: new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("95e52dc4-44f5-4b0b-aabf-4044f28cc55a"),
                column: "FechaCreacion",
                value: new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("a77176f8-d33a-4c23-b613-f2e73093e1b7"),
                column: "FechaCreacion",
                value: new DateTime(2026, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
    }
}
