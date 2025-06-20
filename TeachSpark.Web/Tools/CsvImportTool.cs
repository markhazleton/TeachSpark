using Microsoft.EntityFrameworkCore;
using TeachSpark.Web.Data;
using TeachSpark.Web.Services;

namespace TeachSpark.Web.Tools;

/// <summary>
/// Console tool for importing academic standards from CSV
/// </summary>
public class CsvImportTool
{
    public static async Task ImportAcademicStandardsAsync()
    {
        // Create a DbContext
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlite("Data Source=app.db");

        using var context = new ApplicationDbContext(optionsBuilder.Options);

        // Create logger
        using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<AcademicStandardsImportService>();

        // Create import service
        var importService = new AcademicStandardsImportService(context, logger);

        // Import data
        var csvFilePath = @"c:\GitHub\MarkHazleton\TeachSpark\TeachSpark.Web\Data\2020cas-rw.csv";

        try
        {
            var importedCount = await importService.ImportFromCsvAsync(csvFilePath);
            Console.WriteLine($"Successfully imported {importedCount} academic standards.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error importing CSV: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }
}

/// <summary>
/// Program entry point for running import tool
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Academic Standards CSV Import Tool");
        Console.WriteLine("==================================");

        await CsvImportTool.ImportAcademicStandardsAsync();

        Console.WriteLine("Import complete. Press any key to exit...");
        Console.ReadKey();
    }
}
