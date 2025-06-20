using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachSpark.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddAcademicStandardsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AcademicStandardId",
                table: "Worksheets",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AcademicStandards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Subject = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Grade = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Standard = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    PreparedGraduate1 = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    PreparedGraduate2 = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    PreparedGraduate3 = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    PreparedGraduate4 = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    PreparedGraduate5 = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    GradeLevelExpectation = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    GleCode = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Subheading = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Statement = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AcademicStandards", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Worksheets_AcademicStandardId",
                table: "Worksheets",
                column: "AcademicStandardId");

            migrationBuilder.CreateIndex(
                name: "IX_AcademicStandards_GleCode",
                table: "AcademicStandards",
                column: "GleCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AcademicStandards_Subject_Grade",
                table: "AcademicStandards",
                columns: new[] { "Subject", "Grade" });

            migrationBuilder.AddForeignKey(
                name: "FK_Worksheets_AcademicStandards_AcademicStandardId",
                table: "Worksheets",
                column: "AcademicStandardId",
                principalTable: "AcademicStandards",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Worksheets_AcademicStandards_AcademicStandardId",
                table: "Worksheets");

            migrationBuilder.DropTable(
                name: "AcademicStandards");

            migrationBuilder.DropIndex(
                name: "IX_Worksheets_AcademicStandardId",
                table: "Worksheets");

            migrationBuilder.DropColumn(
                name: "AcademicStandardId",
                table: "Worksheets");
        }
    }
}
