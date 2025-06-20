using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachSpark.Web.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUniqueConstraintFromGleCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AcademicStandards_GleCode",
                table: "AcademicStandards");

            migrationBuilder.CreateIndex(
                name: "IX_AcademicStandards_GleCode",
                table: "AcademicStandards",
                column: "GleCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AcademicStandards_GleCode",
                table: "AcademicStandards");

            migrationBuilder.CreateIndex(
                name: "IX_AcademicStandards_GleCode",
                table: "AcademicStandards",
                column: "GleCode",
                unique: true);
        }
    }
}
