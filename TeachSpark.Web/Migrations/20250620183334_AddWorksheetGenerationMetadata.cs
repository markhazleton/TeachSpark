using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachSpark.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddWorksheetGenerationMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ContentJson",
                table: "Worksheets",
                newName: "ContentMarkdown");

            migrationBuilder.AddColumn<double>(
                name: "ConfidenceScore",
                table: "Worksheets",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EstimatedDurationMinutes",
                table: "Worksheets",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "HasAnswerKey",
                table: "Worksheets",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "QuestionCount",
                table: "Worksheets",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RenderedHtml",
                table: "Worksheets",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TokensUsed",
                table: "Worksheets",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Warnings",
                table: "Worksheets",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConfidenceScore",
                table: "Worksheets");

            migrationBuilder.DropColumn(
                name: "EstimatedDurationMinutes",
                table: "Worksheets");

            migrationBuilder.DropColumn(
                name: "HasAnswerKey",
                table: "Worksheets");

            migrationBuilder.DropColumn(
                name: "QuestionCount",
                table: "Worksheets");

            migrationBuilder.DropColumn(
                name: "RenderedHtml",
                table: "Worksheets");

            migrationBuilder.DropColumn(
                name: "TokensUsed",
                table: "Worksheets");

            migrationBuilder.DropColumn(
                name: "Warnings",
                table: "Worksheets");

            migrationBuilder.RenameColumn(
                name: "ContentMarkdown",
                table: "Worksheets",
                newName: "ContentJson");
        }
    }
}
