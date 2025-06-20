using Microsoft.EntityFrameworkCore;
using TeachSpark.Web.Data;

namespace TeachSpark.Web.Tools
{
    public class DatabaseCheck
    {
        // Commented out to avoid conflicts with top-level statements in Program.cs
        // Uncomment and run separately if needed as a standalone tool
        /*
        public static async Task Main(string[] args)
        {
            Console.WriteLine("TeachSpark Database Check Tool");
            Console.WriteLine("==============================");

            var connectionString = "Data Source=c:\\websites\\teachspark\\teachspark.db";

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connectionString)
                .Options;

            try
            {
                using var context = new ApplicationDbContext(options);

                Console.WriteLine("Checking database connection...");
                await context.Database.OpenConnectionAsync();
                Console.WriteLine("✓ Database connection successful");

                Console.WriteLine("\nChecking worksheets table...");
                var worksheetCount = await context.Worksheets.CountAsync();
                Console.WriteLine($"Total worksheets in database: {worksheetCount}");

                if (worksheetCount > 0)
                {
                    Console.WriteLine("\nRecent worksheets:");
                    var recentWorksheets = await context.Worksheets
                        .OrderByDescending(w => w.CreatedAt)
                        .Take(5)
                        .Select(w => new
                        {
                            w.Id,
                            w.Title,
                            w.UserId,
                            w.WorksheetType,
                            w.CreatedAt,
                            HasContent = !string.IsNullOrEmpty(w.ContentMarkdown),
                            w.LlmModel,
                            w.TokensUsed,
                            w.QuestionCount
                        })
                        .ToListAsync();

                    foreach (var worksheet in recentWorksheets)
                    {
                        Console.WriteLine($"  ID: {worksheet.Id}, Title: {worksheet.Title}");
                        Console.WriteLine($"    User: {worksheet.UserId}, Type: {worksheet.WorksheetType}");
                        Console.WriteLine($"    Created: {worksheet.CreatedAt}, Has Content: {worksheet.HasContent}");
                        Console.WriteLine($"    LLM Model: {worksheet.LlmModel ?? "N/A"}, Tokens: {worksheet.TokensUsed ?? 0}");
                        Console.WriteLine();
                    }
                }

                Console.WriteLine("\nChecking Common Core Standards...");
                var standardsCount = await context.CommonCoreStandards.CountAsync();
                Console.WriteLine($"Total Common Core Standards: {standardsCount}");

                Console.WriteLine("\nChecking Bloom Levels...");
                var bloomCount = await context.BloomLevels.CountAsync();
                Console.WriteLine($"Total Bloom Levels: {bloomCount}");

                Console.WriteLine("\nDatabase check completed successfully!");
            }
            catch (Exception ex)            {
                Console.WriteLine($"❌ Error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
        */
    }
}
