using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using TeachSpark.Web.Data;
using TeachSpark.Web.Data.Entities;

namespace TeachSpark.Web.Areas.Admin.Controllers
{
    public class CommonCoreStandardsController : BaseAdminController
    {
        private readonly ApplicationDbContext _context;

        public CommonCoreStandardsController(ApplicationDbContext context)
        {
            _context = context;
        }        // GET: Admin/CommonCoreStandards
        public IActionResult Index()
        {
            ViewData["Title"] = "Common Core Standards Management";
            return View();
        }

        // GET: Admin/CommonCoreStandards/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var standard = await _context.CommonCoreStandards
                .Include(s => s.Worksheets)
                    .ThenInclude(w => w.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (standard == null)
            {
                return NotFound();
            }

            ViewData["Title"] = $"Standard Details - {standard.Code}";
            return View(standard);
        }

        // GET: Admin/CommonCoreStandards/Create
        public IActionResult Create()
        {
            ViewData["Title"] = "Create New Common Core Standard";
            return View();
        }

        // POST: Admin/CommonCoreStandards/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Code,Grade,Subject,Domain,Description,Category,ExampleActivities,IsActive,SortOrder")] CommonCoreStandard standard)
        {
            if (ModelState.IsValid)
            {
                // Check if code already exists
                var existingCode = await _context.CommonCoreStandards.AnyAsync(s => s.Code == standard.Code);
                if (existingCode)
                {
                    ModelState.AddModelError("Code", "A standard with this code already exists.");
                    ViewData["Title"] = "Create New Common Core Standard";
                    return View(standard);
                }

                _context.Add(standard);
                await _context.SaveChangesAsync();
                SetSuccessMessage($"Common Core Standard '{standard.Code}' has been created successfully.");
                return RedirectToAction(nameof(Index));
            }
            ViewData["Title"] = "Create New Common Core Standard";
            return View(standard);
        }

        // GET: Admin/CommonCoreStandards/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var standard = await _context.CommonCoreStandards.FindAsync(id);
            if (standard == null)
            {
                return NotFound();
            }

            ViewData["Title"] = $"Edit Standard - {standard.Code}";
            return View(standard);
        }

        // POST: Admin/CommonCoreStandards/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Code,Grade,Subject,Domain,Description,Category,ExampleActivities,IsActive,SortOrder")] CommonCoreStandard standard)
        {
            if (id != standard.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Check if code already exists (excluding current record)
                    var existingCode = await _context.CommonCoreStandards
                        .AnyAsync(s => s.Code == standard.Code && s.Id != standard.Id);
                    if (existingCode)
                    {
                        ModelState.AddModelError("Code", "A standard with this code already exists.");
                        ViewData["Title"] = $"Edit Standard - {standard.Code}";
                        return View(standard);
                    }

                    _context.Update(standard);
                    await _context.SaveChangesAsync();
                    SetSuccessMessage($"Common Core Standard '{standard.Code}' has been updated successfully.");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CommonCoreStandardExists(standard.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["Title"] = $"Edit Standard - {standard.Code}";
            return View(standard);
        }

        // GET: Admin/CommonCoreStandards/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var standard = await _context.CommonCoreStandards
                .Include(s => s.Worksheets)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (standard == null)
            {
                return NotFound();
            }

            ViewData["Title"] = $"Delete Standard - {standard.Code}";
            return View(standard);
        }

        // POST: Admin/CommonCoreStandards/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var standard = await _context.CommonCoreStandards
                .Include(s => s.Worksheets)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (standard != null)
            {
                if (standard.Worksheets.Any())
                {
                    SetErrorMessage($"Cannot delete Standard '{standard.Code}' because it is used by {standard.Worksheets.Count} worksheet(s).");
                    return RedirectToAction(nameof(Delete), new { id });
                }

                _context.CommonCoreStandards.Remove(standard);
                await _context.SaveChangesAsync();
                SetSuccessMessage($"Common Core Standard '{standard.Code}' has been deleted successfully.");
            }

            return RedirectToAction(nameof(Index));
        }

        private bool CommonCoreStandardExists(int id)
        {
            return _context.CommonCoreStandards.Any(e => e.Id == id);
        }        // API endpoint for DataTables
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetStandardsData()
        {
            var standards = await _context.CommonCoreStandards
                .Include(s => s.Worksheets)
                .OrderBy(s => s.SortOrder)
                .ThenBy(s => s.Code)
                .Select(s => new
                {
                    id = s.Id,
                    code = s.Code,
                    grade = s.Grade,
                    subject = s.Subject,
                    domain = s.Domain,
                    category = s.Category ?? string.Empty,
                    description = s.Description != null && s.Description.Length > 100 ? s.Description.Substring(0, 100) + "..." : s.Description ?? string.Empty,
                    isActive = s.IsActive,
                    worksheetCount = s.Worksheets.Count,
                    sortOrder = s.SortOrder
                })
                .ToListAsync();

            return Json(new { data = standards });
        }

        // GET: Admin/CommonCoreStandards/ImportCsv
        public IActionResult ImportCsv()
        {
            ViewData["Title"] = "Import Common Core Standards from CSV";
            return View();
        }

        // POST: Admin/CommonCoreStandards/ImportCsv
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportCsv(IFormFile? csvFile, string defaultSubject = "English Language Arts")
        {
            if (csvFile == null || csvFile.Length == 0)
            {
                SetErrorMessage("Please select a CSV file to import.");
                ViewData["Title"] = "Import Common Core Standards from CSV";
                return View();
            }

            if (!csvFile.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                SetErrorMessage("Please select a valid CSV file.");
                ViewData["Title"] = "Import Common Core Standards from CSV";
                return View();
            }

            try
            {
                var standards = new List<CommonCoreStandard>();
                var sortOrder = 1;

                using (var reader = new StreamReader(csvFile.OpenReadStream(), Encoding.UTF8))
                {
                    // Skip header line
                    await reader.ReadLineAsync();

                    string? line;
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        var columns = ParseCsvLine(line);
                        if (columns.Length >= 4)
                        {
                            var code = columns[0].Trim();
                            var domain = columns[1].Trim();
                            var category = columns[2].Trim();
                            var description = columns[3].Trim();

                            // Extract grade from code (e.g., "K.RL.1" -> "K", "11-12.WHST.10" -> "11-12")
                            var grade = ExtractGradeFromCode(code);

                            var standard = new CommonCoreStandard
                            {
                                Code = code,
                                Grade = grade,
                                Subject = defaultSubject,
                                Domain = domain,
                                Category = category,
                                Description = description,
                                ExampleActivities = null,
                                IsActive = true,
                                SortOrder = sortOrder++
                            };

                            standards.Add(standard);
                        }
                    }
                }

                if (standards.Any())
                {
                    // Remove all existing standards
                    var existingStandards = await _context.CommonCoreStandards.ToListAsync();
                    _context.CommonCoreStandards.RemoveRange(existingStandards);

                    // Add new standards
                    await _context.CommonCoreStandards.AddRangeAsync(standards);
                    await _context.SaveChangesAsync();

                    SetSuccessMessage($"Successfully imported {standards.Count} Common Core Standards. All existing standards were replaced.");
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    SetErrorMessage("No valid standards found in the CSV file.");
                }
            }
            catch (Exception ex)
            {
                SetErrorMessage($"Error importing CSV file: {ex.Message}");
            }

            ViewData["Title"] = "Import Common Core Standards from CSV";
            return View();
        }

        // POST: Admin/CommonCoreStandards/ImportFromDefaultCsv
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportFromDefaultCsv(string defaultSubject = "English Language Arts")
        {
            try
            {
                var csvPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "CCSS Common Core Standards - English Standards.csv");

                if (!System.IO.File.Exists(csvPath))
                {
                    SetErrorMessage("Default CSV file not found. Please upload a CSV file instead.");
                    return RedirectToAction(nameof(ImportCsv));
                }

                var standards = new List<CommonCoreStandard>();
                var sortOrder = 1;

                var lines = await System.IO.File.ReadAllLinesAsync(csvPath, Encoding.UTF8);

                // Skip header line
                for (int i = 1; i < lines.Length; i++)
                {
                    var columns = ParseCsvLine(lines[i]);
                    if (columns.Length >= 4)
                    {
                        var code = columns[0].Trim();
                        var domain = columns[1].Trim();
                        var category = columns[2].Trim();
                        var description = columns[3].Trim();

                        // Extract grade from code
                        var grade = ExtractGradeFromCode(code);

                        var standard = new CommonCoreStandard
                        {
                            Code = code,
                            Grade = grade,
                            Subject = defaultSubject,
                            Domain = domain,
                            Category = category,
                            Description = description,
                            ExampleActivities = null,
                            IsActive = true,
                            SortOrder = sortOrder++
                        };

                        standards.Add(standard);
                    }
                }

                if (standards.Any())
                {
                    // Remove all existing standards
                    var existingStandards = await _context.CommonCoreStandards.ToListAsync();
                    _context.CommonCoreStandards.RemoveRange(existingStandards);

                    // Add new standards
                    await _context.CommonCoreStandards.AddRangeAsync(standards);
                    await _context.SaveChangesAsync();

                    SetSuccessMessage($"Successfully imported {standards.Count} Common Core Standards from the default CSV file. All existing standards were replaced.");
                }
                else
                {
                    SetErrorMessage("No valid standards found in the default CSV file.");
                }
            }
            catch (Exception ex)
            {
                SetErrorMessage($"Error importing from default CSV file: {ex.Message}");
            }

            return RedirectToAction(nameof(Index));
        }

        private static string ExtractGradeFromCode(string code)
        {
            if (string.IsNullOrEmpty(code))
                return string.Empty;

            // Handle patterns like "K.RL.1", "1.RL.2", "11-12.WHST.10"
            var dotIndex = code.IndexOf('.');
            if (dotIndex > 0)
            {
                return code.Substring(0, dotIndex);
            }

            return string.Empty;
        }

        private static string[] ParseCsvLine(string line)
        {
            var result = new List<string>();
            var currentField = new StringBuilder();
            var inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        // Double quote inside quoted field
                        currentField.Append('"');
                        i++; // Skip next quote
                    }
                    else
                    {
                        // Toggle quote state
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    // Field separator
                    result.Add(currentField.ToString());
                    currentField.Clear();
                }
                else
                {
                    currentField.Append(c);
                }
            }

            // Add the last field
            result.Add(currentField.ToString());

            return result.ToArray();
        }
    }
}
