using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeachSpark.Web.Data;
using TeachSpark.Web.Services;

namespace TeachSpark.Web.Areas.Admin.Controllers
{
    public class AcademicStandardsController : BaseAdminController
    {
        private readonly ApplicationDbContext _context;
        private readonly AcademicStandardsImportService _importService;
        private readonly ILogger<AcademicStandardsController> _logger;

        public AcademicStandardsController(
            ApplicationDbContext context,
            AcademicStandardsImportService importService,
            ILogger<AcademicStandardsController> logger)
        {
            _context = context;
            _importService = importService;
            _logger = logger;
        }

        // GET: Admin/AcademicStandards
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Academic Standards Management";

            var standards = await _context.AcademicStandards
                .OrderBy(a => a.Subject)
                .ThenBy(a => a.Grade)
                .ThenBy(a => a.GleCode)
                .ToListAsync();

            return View(standards);
        }

        // GET: Admin/AcademicStandards/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var standard = await _context.AcademicStandards
                .Include(a => a.Worksheets)
                    .ThenInclude(w => w.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (standard == null)
            {
                return NotFound();
            }

            ViewData["Title"] = $"Academic Standard Details - {standard.GleCode}";
            return View(standard);
        }

        // POST: Admin/AcademicStandards/RefreshFromCsv
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RefreshFromCsv()
        {
            try
            {
                // Path to the CSV file in the project
                var csvFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "2020cas-rw.csv");

                if (!System.IO.File.Exists(csvFilePath))
                {
                    TempData["ErrorMessage"] = $"CSV file not found at: {csvFilePath}";
                    return RedirectToAction(nameof(Index));
                }

                // Clear existing data
                var existingStandards = await _context.AcademicStandards.ToListAsync();
                _context.AcademicStandards.RemoveRange(existingStandards);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Cleared {Count} existing academic standards", existingStandards.Count);

                // Import fresh data from CSV
                var importedCount = await _importService.ImportFromCsvAsync(csvFilePath);

                TempData["SuccessMessage"] = $"Successfully refreshed data! Removed {existingStandards.Count} old records and imported {importedCount} new academic standards from CSV.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing academic standards from CSV");
                var innerException = ex.InnerException != null ? $" Inner: {ex.InnerException.Message}" : string.Empty;
                TempData["ErrorMessage"] = $"Error refreshing data: {ex.Message}{innerException}";
                return RedirectToAction(nameof(Index));
            }
        }

        // API endpoint for DataTables
        [HttpPost]
        public async Task<IActionResult> GetStandardsData()
        {
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();
            var sortColumn = Request.Form["order[0][column]"].FirstOrDefault();
            var sortDirection = Request.Form["order[0][dir]"].FirstOrDefault();

            // Custom filter parameters
            var subjectFilter = Request.Form["subjectFilter"].FirstOrDefault();
            var gradeFilter = Request.Form["gradeFilter"].FirstOrDefault();
            var customSearch = Request.Form["customSearch"].FirstOrDefault();

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            var query = _context.AcademicStandards.AsQueryable();

            // Apply subject filter
            if (!string.IsNullOrEmpty(subjectFilter))
            {
                query = query.Where(s => s.Subject == subjectFilter);
            }

            // Apply grade filter
            if (!string.IsNullOrEmpty(gradeFilter))
            {
                query = query.Where(s => s.Grade == gradeFilter);
            }

            // Apply custom search filter (more comprehensive than default search)
            if (!string.IsNullOrEmpty(customSearch))
            {
                var searchLower = customSearch.ToLower();
                query = query.Where(s => s.GleCode.ToLower().Contains(searchLower) ||
                                       s.Statement.ToLower().Contains(searchLower) ||
                                       s.Subheading.ToLower().Contains(searchLower));
            }

            // Apply default search filter if no custom search
            if (string.IsNullOrEmpty(customSearch) && !string.IsNullOrEmpty(searchValue))
            {
                query = query.Where(s => s.Subject.Contains(searchValue) ||
                                       s.Grade.Contains(searchValue) ||
                                       s.GleCode.Contains(searchValue) ||
                                       s.Statement.Contains(searchValue));
            }

            // Apply sorting
            if (!string.IsNullOrEmpty(sortColumn))
            {
                var columnIndex = Convert.ToInt32(sortColumn);
                switch (columnIndex)
                {
                    case 0: // Subject
                        query = sortDirection == "asc" ? query.OrderBy(s => s.Subject) : query.OrderByDescending(s => s.Subject);
                        break;
                    case 1: // Grade
                        query = sortDirection == "asc" ? query.OrderBy(s => s.Grade) : query.OrderByDescending(s => s.Grade);
                        break;
                    case 2: // GLE Code
                        query = sortDirection == "asc" ? query.OrderBy(s => s.GleCode) : query.OrderByDescending(s => s.GleCode);
                        break;
                    case 3: // Statement
                        query = sortDirection == "asc" ? query.OrderBy(s => s.Statement) : query.OrderByDescending(s => s.Statement);
                        break;
                    default:
                        query = query.OrderBy(s => s.Subject).ThenBy(s => s.Grade);
                        break;
                }
            }
            else
            {
                query = query.OrderBy(s => s.Subject).ThenBy(s => s.Grade).ThenBy(s => s.GleCode);
            }

            var totalRecords = await _context.AcademicStandards.CountAsync();
            var filteredRecords = await query.CountAsync();

            var data = await query.Skip(skip).Take(pageSize).ToListAsync();

            return Json(new
            {
                draw = draw,
                recordsTotal = totalRecords,
                recordsFiltered = filteredRecords,
                data = data.Select(s => new
                {
                    id = s.Id,
                    subject = s.Subject,
                    grade = s.Grade,
                    gleCode = s.GleCode,
                    statement = s.Statement.Length > 100 ? s.Statement.Substring(0, 100) + "..." : s.Statement,
                    subheading = string.IsNullOrEmpty(s.Subheading) ? string.Empty : (s.Subheading.Length > 50 ? s.Subheading.Substring(0, 50) + "..." : s.Subheading)
                })
            });
        }

        // API endpoint to get filter options
        [HttpGet]
        public async Task<IActionResult> GetFilterOptions()
        {
            try
            {
                var subjects = await _context.AcademicStandards
                    .Select(s => s.Subject)
                    .Distinct()
                    .OrderBy(s => s)
                    .ToListAsync();

                var grades = await _context.AcademicStandards
                    .Select(s => s.Grade)
                    .Distinct()
                    .OrderBy(g => g)
                    .ToListAsync();

                return Json(new
                {
                    subjects = subjects,
                    grades = grades
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting filter options");
                return Json(new { subjects = new List<string>(), grades = new List<string>() });
            }
        }

        // GET: Admin/AcademicStandards/TestImport - Simple test endpoint
        [HttpGet]
        public async Task<IActionResult> TestImport()
        {
            try
            {
                // Path to the CSV file in the project
                var csvFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "2020cas-rw.csv");

                if (!System.IO.File.Exists(csvFilePath))
                {
                    return Json(new { success = false, message = $"CSV file not found at: {csvFilePath}" });
                }

                // Clear existing data
                var existingStandards = await _context.AcademicStandards.ToListAsync();
                _context.AcademicStandards.RemoveRange(existingStandards);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Cleared {Count} existing academic standards", existingStandards.Count);

                // Import fresh data from CSV
                var importedCount = await _importService.ImportFromCsvAsync(csvFilePath);

                return Json(new
                {
                    success = true,
                    message = $"Successfully refreshed data! Removed {existingStandards.Count} old records and imported {importedCount} new academic standards from CSV.",
                    removedCount = existingStandards.Count,
                    importedCount = importedCount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing academic standards from CSV");
                var innerException = ex.InnerException != null ? $" Inner: {ex.InnerException.Message}" : string.Empty;
                return Json(new { success = false, message = $"Error refreshing data: {ex.Message}{innerException}" });
            }
        }
    }
}
