using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachSpark.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddPromptTemplatesToWorksheetTemplate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SystemPromptTemplate",
                table: "WorksheetTemplates",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserPromptTemplate",
                table: "WorksheetTemplates",
                type: "TEXT",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "WorksheetTemplates",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "SystemPromptTemplate", "UserPromptTemplate" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "WorksheetTemplates",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "SystemPromptTemplate", "UserPromptTemplate" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "WorksheetTemplates",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "SystemPromptTemplate", "UserPromptTemplate" },
                values: new object[] { null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SystemPromptTemplate",
                table: "WorksheetTemplates");

            migrationBuilder.DropColumn(
                name: "UserPromptTemplate",
                table: "WorksheetTemplates");
        }
    }
}
