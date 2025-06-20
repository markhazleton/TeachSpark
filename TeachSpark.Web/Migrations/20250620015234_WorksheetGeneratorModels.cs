using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TeachSpark.Web.Migrations
{
    /// <inheritdoc />
    public partial class WorksheetGeneratorModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "AspNetUsers",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                table: "AspNetUsers",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "District",
                table: "AspNetUsers",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GradeLevel",
                table: "AspNetUsers",
                type: "TEXT",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "School",
                table: "AspNetUsers",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ApiKeys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    KeyValue = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    KeyPrefix = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastUsedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RequestCount = table.Column<int>(type: "INTEGER", nullable: false),
                    DailyRequestLimit = table.Column<int>(type: "INTEGER", nullable: false),
                    MonthlyRequestLimit = table.Column<int>(type: "INTEGER", nullable: false),
                    AllowedIpAddresses = table.Column<string>(type: "TEXT", nullable: true),
                    Scopes = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiKeys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApiKeys_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BloomLevels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false),
                    ActionVerbs = table.Column<string>(type: "TEXT", nullable: false),
                    Examples = table.Column<string>(type: "TEXT", nullable: true),
                    ColorCode = table.Column<string>(type: "TEXT", maxLength: 7, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BloomLevels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CommonCoreStandards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Grade = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Subject = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Domain = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ExampleActivities = table.Column<string>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommonCoreStandards", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorksheetTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    TemplateType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    LayoutJson = table.Column<string>(type: "TEXT", nullable: false),
                    PreviewImageUrl = table.Column<string>(type: "TEXT", nullable: true),
                    IsPublic = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsSystem = table.Column<bool>(type: "INTEGER", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UsageCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorksheetTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorksheetTemplates_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ApiUsages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ApiKeyId = table.Column<int>(type: "INTEGER", nullable: false),
                    Endpoint = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    HttpMethod = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    IpAddress = table.Column<string>(type: "TEXT", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ResponseStatusCode = table.Column<int>(type: "INTEGER", nullable: false),
                    ResponseTime = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    TokensUsed = table.Column<int>(type: "INTEGER", nullable: true),
                    CostIncurred = table.Column<decimal>(type: "TEXT", nullable: true),
                    RequestedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ErrorMessage = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiUsages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApiUsages_ApiKeys_ApiKeyId",
                        column: x => x.ApiKeyId,
                        principalTable: "ApiKeys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Worksheets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    CommonCoreStandardId = table.Column<int>(type: "INTEGER", nullable: true),
                    BloomLevelId = table.Column<int>(type: "INTEGER", nullable: true),
                    TemplateId = table.Column<int>(type: "INTEGER", nullable: true),
                    ContentJson = table.Column<string>(type: "TEXT", nullable: false),
                    SourceText = table.Column<string>(type: "TEXT", nullable: true),
                    WorksheetType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    DifficultyLevel = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    AccessibilityOptions = table.Column<string>(type: "TEXT", nullable: true),
                    Tags = table.Column<string>(type: "TEXT", nullable: true),
                    IsPublic = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsFavorite = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastAccessedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ViewCount = table.Column<int>(type: "INTEGER", nullable: false),
                    DownloadCount = table.Column<int>(type: "INTEGER", nullable: false),
                    LlmModel = table.Column<string>(type: "TEXT", nullable: true),
                    GenerationPrompt = table.Column<string>(type: "TEXT", nullable: true),
                    GenerationCost = table.Column<decimal>(type: "TEXT", nullable: true),
                    GenerationTime = table.Column<TimeSpan>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Worksheets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Worksheets_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Worksheets_BloomLevels_BloomLevelId",
                        column: x => x.BloomLevelId,
                        principalTable: "BloomLevels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Worksheets_CommonCoreStandards_CommonCoreStandardId",
                        column: x => x.CommonCoreStandardId,
                        principalTable: "CommonCoreStandards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Worksheets_WorksheetTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "WorksheetTemplates",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "WorksheetExports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    WorksheetId = table.Column<int>(type: "INTEGER", nullable: false),
                    ExportFormat = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    FilePath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    FileName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    FileSizeBytes = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsTemporary = table.Column<bool>(type: "INTEGER", nullable: false),
                    DownloadCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorksheetExports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorksheetExports_Worksheets_WorksheetId",
                        column: x => x.WorksheetId,
                        principalTable: "Worksheets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "BloomLevels",
                columns: new[] { "Id", "ActionVerbs", "ColorCode", "Description", "Examples", "Name", "Order" },
                values: new object[,]
                {
                    { 1, "define, duplicate, list, memorize, recall, repeat, reproduce, state, identify, name, recognize, select, match", "#6f42c1", "Retrieving, recognizing, and recalling relevant knowledge from long-term memory", "Define literary terms, list character names, recall plot events", "Remember", 1 },
                    { 2, "classify, describe, discuss, explain, express, identify, indicate, locate, recognize, report, restate, review, select, translate, paraphrase, summarize", "#0d6efd", "Constructing meaning from oral, written, and graphic messages", "Explain theme, summarize plot, describe character traits", "Understand", 2 },
                    { 3, "apply, carry out, demonstrate, dramatize, employ, illustrate, interpret, operate, practice, schedule, sketch, solve, use, write, implement", "#198754", "Carrying out or using a procedure through executing or implementing", "Apply grammar rules, use literary devices, implement writing strategies", "Apply", 3 },
                    { 4, "analyze, appraise, calculate, categorize, compare, contrast, criticize, differentiate, discriminate, distinguish, examine, experiment, question, test, deconstruct", "#fd7e14", "Breaking material into constituent parts and determining how parts relate to each other", "Compare characters, analyze author's purpose, examine text structure", "Analyze", 4 },
                    { 5, "appraise, argue, assess, attach, choose, compare, defend, estimate, judge, predict, rate, select, support, value, evaluate, critique", "#dc3545", "Making judgments based on criteria and standards through checking and critiquing", "Critique author's argument, evaluate source credibility, judge character decisions", "Evaluate", 5 },
                    { 6, "arrange, assemble, collect, compose, construct, create, design, develop, formulate, manage, organize, plan, prepare, propose, set up, write, generate", "#20c997", "Putting elements together to form a coherent or functional whole; reorganizing into a new pattern", "Write alternative ending, create character analysis, design presentation", "Create", 6 }
                });

            migrationBuilder.InsertData(
                table: "CommonCoreStandards",
                columns: new[] { "Id", "Category", "Code", "Description", "Domain", "ExampleActivities", "Grade", "IsActive", "SortOrder", "Subject" },
                values: new object[,]
                {
                    { 1, "Key Ideas and Details", "CCSS.ELA-LITERACY.RL.8.1", "Cite the textual evidence that most strongly supports an analysis of what the text says explicitly as well as inferences drawn from the text.", "Reading: Literature", null, "8", true, 1, "English Language Arts" },
                    { 2, "Key Ideas and Details", "CCSS.ELA-LITERACY.RL.8.2", "Determine a theme or central idea of a text and analyze its development over the course of the text, including its relationship to the characters, setting, and plot.", "Reading: Literature", null, "8", true, 2, "English Language Arts" },
                    { 3, "Key Ideas and Details", "CCSS.ELA-LITERACY.RL.8.3", "Analyze how particular lines of dialogue or incidents in a story or drama propel the action, reveal aspects of a character, or provoke a decision.", "Reading: Literature", null, "8", true, 3, "English Language Arts" },
                    { 4, "Text Types and Purposes", "CCSS.ELA-LITERACY.W.8.1", "Write arguments to support claims with clear reasons and relevant evidence.", "Writing", null, "8", true, 4, "English Language Arts" },
                    { 5, "Conventions of Standard English", "CCSS.ELA-LITERACY.L.8.1", "Demonstrate command of the conventions of standard English grammar and usage when writing or speaking.", "Language", null, "8", true, 5, "English Language Arts" }
                });

            migrationBuilder.InsertData(
                table: "WorksheetTemplates",
                columns: new[] { "Id", "CreatedAt", "Description", "IsPublic", "IsSystem", "LayoutJson", "Name", "PreviewImageUrl", "TemplateType", "UpdatedAt", "UsageCount", "UserId" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 6, 20, 1, 52, 33, 545, DateTimeKind.Utc).AddTicks(4565), "Standard reading comprehension worksheet with passage and questions", true, true, "{\"sections\": [{\"type\": \"passage\", \"title\": \"Reading Passage\"}, {\"type\": \"questions\", \"title\": \"Comprehension Questions\", \"questionCount\": 5}]}", "Reading Comprehension - Basic", null, "reading-comprehension", new DateTime(2025, 6, 20, 1, 52, 33, 545, DateTimeKind.Utc).AddTicks(4570), 0, null },
                    { 2, new DateTime(2025, 6, 20, 1, 52, 33, 545, DateTimeKind.Utc).AddTicks(5164), "Vocabulary worksheet with definitions and context practice", true, true, "{\"sections\": [{\"type\": \"vocabulary\", \"title\": \"New Words\"}, {\"type\": \"definitions\", \"title\": \"Match Definitions\"}, {\"type\": \"sentences\", \"title\": \"Use in Sentences\"}]}", "Vocabulary Builder", null, "vocabulary", new DateTime(2025, 6, 20, 1, 52, 33, 545, DateTimeKind.Utc).AddTicks(5164), 0, null },
                    { 3, new DateTime(2025, 6, 20, 1, 52, 33, 545, DateTimeKind.Utc).AddTicks(5165), "Grammar exercises with examples and practice problems", true, true, "{\"sections\": [{\"type\": \"explanation\", \"title\": \"Grammar Rule\"}, {\"type\": \"examples\", \"title\": \"Examples\"}, {\"type\": \"practice\", \"title\": \"Practice Exercises\"}]}", "Grammar Practice", null, "grammar", new DateTime(2025, 6, 20, 1, 52, 33, 545, DateTimeKind.Utc).AddTicks(5166), 0, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApiKeys_KeyValue",
                table: "ApiKeys",
                column: "KeyValue",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApiKeys_UserId",
                table: "ApiKeys",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ApiUsages_ApiKeyId",
                table: "ApiUsages",
                column: "ApiKeyId");

            migrationBuilder.CreateIndex(
                name: "IX_ApiUsages_RequestedAt",
                table: "ApiUsages",
                column: "RequestedAt");

            migrationBuilder.CreateIndex(
                name: "IX_BloomLevels_Order",
                table: "BloomLevels",
                column: "Order",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CommonCoreStandards_Code",
                table: "CommonCoreStandards",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorksheetExports_WorksheetId",
                table: "WorksheetExports",
                column: "WorksheetId");

            migrationBuilder.CreateIndex(
                name: "IX_Worksheets_BloomLevelId",
                table: "Worksheets",
                column: "BloomLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_Worksheets_CommonCoreStandardId",
                table: "Worksheets",
                column: "CommonCoreStandardId");

            migrationBuilder.CreateIndex(
                name: "IX_Worksheets_CreatedAt",
                table: "Worksheets",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Worksheets_TemplateId",
                table: "Worksheets",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_Worksheets_UserId",
                table: "Worksheets",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorksheetTemplates_UserId",
                table: "WorksheetTemplates",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApiUsages");

            migrationBuilder.DropTable(
                name: "WorksheetExports");

            migrationBuilder.DropTable(
                name: "ApiKeys");

            migrationBuilder.DropTable(
                name: "Worksheets");

            migrationBuilder.DropTable(
                name: "BloomLevels");

            migrationBuilder.DropTable(
                name: "CommonCoreStandards");

            migrationBuilder.DropTable(
                name: "WorksheetTemplates");

            migrationBuilder.DropColumn(
                name: "District",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "GradeLevel",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "School",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "AspNetUsers",
                type: "TEXT",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                table: "AspNetUsers",
                type: "TEXT",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50);
        }
    }
}
