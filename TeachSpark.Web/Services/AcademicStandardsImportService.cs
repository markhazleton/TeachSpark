using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using TeachSpark.Web.Data;
using TeachSpark.Web.Data.Entities;

namespace TeachSpark.Web.Services;

/// <summary>
/// Service for importing academic standards from CSV files
/// </summary>
public class AcademicStandardsImportService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AcademicStandardsImportService> _logger;

    public AcademicStandardsImportService(ApplicationDbContext context, ILogger<AcademicStandardsImportService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Imports academic standards from a CSV file
    /// </summary>
    /// <param name="csvFilePath">Path to the CSV file</param>
    /// <returns>Number of records imported</returns>
    public async Task<int> ImportFromCsvAsync(string csvFilePath)
    {
        if (!File.Exists(csvFilePath))
        {
            throw new FileNotFoundException($"CSV file not found: {csvFilePath}");
        }

        var records = new List<AcademicStandard>();

        using var reader = new StringReader(await File.ReadAllTextAsync(csvFilePath));
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,
            MissingFieldFound = null
        });

        csv.Context.RegisterClassMap<AcademicStandardCsvMap>();
        var csvRecords = csv.GetRecords<AcademicStandardCsv>().ToList(); foreach (var csvRecord in csvRecords)
        {
            // Skip empty rows
            if (string.IsNullOrWhiteSpace(csvRecord.Subject) ||
                string.IsNullOrWhiteSpace(csvRecord.GleCode) ||
                string.IsNullOrWhiteSpace(csvRecord.Statement))
                continue;

            // Check if exact record already exists (by GLE code and statement)
            var existingRecord = await _context.AcademicStandards
                .FirstOrDefaultAsync(a => a.GleCode == csvRecord.GleCode &&
                                        a.Statement == csvRecord.Statement.Trim());

            if (existingRecord != null)
            {
                _logger.LogInformation("Skipping duplicate record: {GleCode} - {Statement}",
                    csvRecord.GleCode, csvRecord.Statement.Substring(0, Math.Min(50, csvRecord.Statement.Length)));
                continue;
            }

            var academicStandard = new AcademicStandard
            {
                Subject = csvRecord.Subject.Trim(),
                Grade = csvRecord.Grade.Trim(),
                Standard = csvRecord.Standard.Trim(),
                PreparedGraduate1 = string.IsNullOrWhiteSpace(csvRecord.PreparedGraduate1) ? null : csvRecord.PreparedGraduate1.Trim(),
                PreparedGraduate2 = string.IsNullOrWhiteSpace(csvRecord.PreparedGraduate2) ? null : csvRecord.PreparedGraduate2.Trim(),
                PreparedGraduate3 = string.IsNullOrWhiteSpace(csvRecord.PreparedGraduate3) ? null : csvRecord.PreparedGraduate3.Trim(),
                PreparedGraduate4 = string.IsNullOrWhiteSpace(csvRecord.PreparedGraduate4) ? null : csvRecord.PreparedGraduate4.Trim(),
                PreparedGraduate5 = string.IsNullOrWhiteSpace(csvRecord.PreparedGraduate5) ? null : csvRecord.PreparedGraduate5.Trim(),
                GradeLevelExpectation = csvRecord.GradeLevelExpectation.Trim(),
                GleCode = csvRecord.GleCode.Trim(),
                Subheading = csvRecord.Subheading.Trim(),
                Statement = csvRecord.Statement.Trim(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            records.Add(academicStandard);
        }

        if (records.Any())
        {
            _context.AcademicStandards.AddRange(records);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Imported {Count} academic standards from CSV", records.Count);
        }

        return records.Count;
    }
}

/// <summary>
/// CSV mapping model for academic standards
/// </summary>
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

/// <summary>
/// CSV class mapping for academic standards
/// </summary>
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
