using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TeachSpark.Web.Migrations
{
    /// <inheritdoc />
    public partial class UpdateWorksheetTemplateStructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Worksheets_WorksheetTemplates_TemplateId",
                table: "Worksheets");

            // Set TemplateId to null for any worksheets that reference the templates we're about to delete
            migrationBuilder.Sql("UPDATE Worksheets SET TemplateId = NULL WHERE TemplateId IN (1, 2, 3);");

            migrationBuilder.DeleteData(
                table: "WorksheetTemplates",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "WorksheetTemplates",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "WorksheetTemplates",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DropColumn(
                name: "LayoutJson",
                table: "WorksheetTemplates");

            migrationBuilder.AddForeignKey(
                name: "FK_Worksheets_WorksheetTemplates_TemplateId",
                table: "Worksheets",
                column: "TemplateId",
                principalTable: "WorksheetTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Worksheets_WorksheetTemplates_TemplateId",
                table: "Worksheets");

            migrationBuilder.AddColumn<string>(
                name: "LayoutJson",
                table: "WorksheetTemplates",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.InsertData(
                table: "WorksheetTemplates",
                columns: new[] { "Id", "CreatedAt", "Description", "IsPublic", "IsSystem", "LayoutJson", "Name", "PreviewImageUrl", "SystemPromptTemplate", "TemplateType", "UpdatedAt", "UsageCount", "UserId", "UserPromptTemplate" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 6, 19, 0, 0, 0, 0, DateTimeKind.Utc), "Standard reading comprehension worksheet with passage and questions", true, true, "{\"sections\": [{\"type\": \"passage\", \"title\": \"Reading Passage\"}, {\"type\": \"questions\", \"title\": \"Comprehension Questions\", \"questionCount\": 5}]}", "Reading Comprehension - Basic", null, null, "reading-comprehension", new DateTime(2025, 6, 19, 0, 0, 0, 0, DateTimeKind.Utc), 0, null, null },
                    { 2, new DateTime(2025, 6, 19, 0, 0, 0, 0, DateTimeKind.Utc), "Vocabulary worksheet with definitions and context practice", true, true, "{\"sections\": [{\"type\": \"vocabulary\", \"title\": \"New Words\"}, {\"type\": \"definitions\", \"title\": \"Match Definitions\"}, {\"type\": \"sentences\", \"title\": \"Use in Sentences\"}]}", "Vocabulary Builder", null, null, "vocabulary", new DateTime(2025, 6, 19, 0, 0, 0, 0, DateTimeKind.Utc), 0, null, null },
                    { 3, new DateTime(2025, 6, 19, 0, 0, 0, 0, DateTimeKind.Utc), "Grammar exercises with examples and practice problems", true, true, "{\"sections\": [{\"type\": \"explanation\", \"title\": \"Grammar Rule\"}, {\"type\": \"examples\", \"title\": \"Examples\"}, {\"type\": \"practice\", \"title\": \"Practice Exercises\"}]}", "Grammar Practice", null, null, "grammar", new DateTime(2025, 6, 19, 0, 0, 0, 0, DateTimeKind.Utc), 0, null, null }
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Worksheets_WorksheetTemplates_TemplateId",
                table: "Worksheets",
                column: "TemplateId",
                principalTable: "WorksheetTemplates",
                principalColumn: "Id");
        }
    }
}
