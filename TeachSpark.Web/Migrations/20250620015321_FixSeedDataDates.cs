using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace TeachSpark.Web.Migrations
{
    /// <inheritdoc />
    public partial class FixSeedDataDates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "WorksheetTemplates",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 6, 19, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 6, 19, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "WorksheetTemplates",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 6, 19, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 6, 19, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "WorksheetTemplates",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 6, 19, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 6, 19, 0, 0, 0, 0, DateTimeKind.Utc) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "WorksheetTemplates",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 6, 20, 1, 52, 33, 545, DateTimeKind.Utc).AddTicks(4565), new DateTime(2025, 6, 20, 1, 52, 33, 545, DateTimeKind.Utc).AddTicks(4570) });

            migrationBuilder.UpdateData(
                table: "WorksheetTemplates",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 6, 20, 1, 52, 33, 545, DateTimeKind.Utc).AddTicks(5164), new DateTime(2025, 6, 20, 1, 52, 33, 545, DateTimeKind.Utc).AddTicks(5164) });

            migrationBuilder.UpdateData(
                table: "WorksheetTemplates",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 6, 20, 1, 52, 33, 545, DateTimeKind.Utc).AddTicks(5165), new DateTime(2025, 6, 20, 1, 52, 33, 545, DateTimeKind.Utc).AddTicks(5166) });
        }
    }
}
