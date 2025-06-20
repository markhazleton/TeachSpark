using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using TeachSpark.Web.Data;
using TeachSpark.Web.Data.Entities;

namespace TeachSpark.Web.Tools
{
    public class CsvImporter
    {
        public static async Task ImportAcademicStandardsCsvAsync(string csvFilePath, string connectionString)
        {
            // Create logger
            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger("CsvImport");

            // Configuration for database connection
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlite(connectionString);

            // Create context
            using var context = new ApplicationDbContext(optionsBuilder.Options);

            try
            {
                logger.LogInformation("Starting CSV import from {FilePath}", csvFilePath);

                if (!File.Exists(csvFilePath))
                {
                    logger.LogError("CSV file not found: {FilePath}", csvFilePath);
                    return;
                }

                var records = new List<AcademicStandard>();

                using var reader = new StringReader(await File.ReadAllTextAsync(csvFilePath));
                using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HeaderValidated = null,
                    MissingFieldFound = null
                });

                csv.Context.RegisterClassMap<AcademicStandardCsvMap>();
                var csvRecords = csv.GetRecords<AcademicStandardCsv>().ToList();

                logger.LogInformation("Found {Count} records in CSV file", csvRecords.Count);

                foreach (var csvRecord in csvRecords)
                {
                    // Skip empty rows
                    if (string.IsNullOrWhiteSpace(csvRecord.Subject) ||
                        string.IsNullOrWhiteSpace(csvRecord.GleCode))
                        continue;

                    // Check if record already exists
                    var existingRecord = await context.AcademicStandards
                        .FirstOrDefaultAsync(a => a.GleCode == csvRecord.GleCode &&
                                                a.Statement == csvRecord.Statement);

                    if (existingRecord != null)
                    {
                        logger.LogInformation("Skipping duplicate record: {GleCode}", csvRecord.GleCode);
                        continue;
                    }

                    var academicStandard = new AcademicStandard
                    {
                        Subject = csvRecord.Subject,
                        Grade = csvRecord.Grade,
                        Standard = csvRecord.Standard,
                        PreparedGraduate1 = string.IsNullOrWhiteSpace(csvRecord.PreparedGraduate1) ? null : csvRecord.PreparedGraduate1,
                        PreparedGraduate2 = string.IsNullOrWhiteSpace(csvRecord.PreparedGraduate2) ? null : csvRecord.PreparedGraduate2,
                        PreparedGraduate3 = string.IsNullOrWhiteSpace(csvRecord.PreparedGraduate3) ? null : csvRecord.PreparedGraduate3,
                        PreparedGraduate4 = string.IsNullOrWhiteSpace(csvRecord.PreparedGraduate4) ? null : csvRecord.PreparedGraduate4,
                        PreparedGraduate5 = string.IsNullOrWhiteSpace(csvRecord.PreparedGraduate5) ? null : csvRecord.PreparedGraduate5,
                        GradeLevelExpectation = csvRecord.GradeLevelExpectation,
                        GleCode = csvRecord.GleCode,
                        Subheading = csvRecord.Subheading,
                        Statement = csvRecord.Statement,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    records.Add(academicStandard);
                }

                if (records.Any())
                {
                    context.AcademicStandards.AddRange(records);
                    await context.SaveChangesAsync();
                    logger.LogInformation("Successfully imported {Count} academic standards", records.Count);
                }
                else
                {
                    logger.LogInformation("No new records to import");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error importing academic standards from CSV");
            }

            logger.LogInformation("Import process completed");
        }
    }

    // CSV mapping classes
    public class AcademicStandardCsv
    {
        public string Subject { get; set; } = string.Empty;
        public string Grade { get; set; } = string.Empty;
        public string Standard { get; set; } = string.Empty;
        public string PreparedGraduate1 { get; set; } = string.Empty;
        public string PreparedGraduate2 { get; set; } = string.Empty;
        public string PreparedGraduate3 { get; set; } = string.Empty;
        public string PreparedGraduate4 { get; set; } = string.Empty;
        public string PreparedGraduate5 { get; set; } = string.Empty;
        public string GradeLevelExpectation { get; set; } = string.Empty;
        public string GleCode { get; set; } = string.Empty;
        public string Subheading { get; set; } = string.Empty;
        public string Statement { get; set; } = string.Empty;
    }

    public class AcademicStandardCsvMap : ClassMap<AcademicStandardCsv>
    {
        public AcademicStandardCsvMap()
        {
            Map(m => m.Subject).Name("Subject");
            Map(m => m.Grade).Name("Grade");
            Map(m => m.Standard).Name("Standard");
            Map(m => m.PreparedGraduate1).Name("Prepared Graduate 1");
            Map(m => m.PreparedGraduate2).Name("Prepared Graduate 2");
            Map(m => m.PreparedGraduate3).Name("Prepared Graduate 3");
            Map(m => m.PreparedGraduate4).Name("Prepared Graduate 4");
            Map(m => m.PreparedGraduate5).Name("Prepared Graduate 5");
            Map(m => m.GradeLevelExpectation).Name("Grade Level Expectation");
            Map(m => m.GleCode).Name("GLE Code");
            Map(m => m.Subheading).Name("Subheading");
            Map(m => m.Statement).Name("Statement");
        }
    }
}
