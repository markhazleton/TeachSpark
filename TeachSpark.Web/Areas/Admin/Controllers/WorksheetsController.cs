using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TeachSpark.Web.Data;
using TeachSpark.Web.Data.Entities;
using TeachSpark.Web.Configuration;

namespace TeachSpark.Web.Areas.Admin.Controllers
{
    public class WorksheetsController : BaseAdminController
    {
        private readonly ApplicationDbContext _context;
        private readonly WorksheetGenerationConfiguration _config;

        public WorksheetsController(ApplicationDbContext context, WorksheetGenerationConfiguration config)
        {
            _context = context;
            _config = config;
        }// GET: Admin/Worksheets
        public IActionResult Index()
        {
            ViewData["Title"] = "Worksheets Management";
            return View();
        }

        // GET: Admin/Worksheets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var worksheet = await _context.Worksheets
                .Include(w => w.User)
                .Include(w => w.BloomLevel)
                .Include(w => w.CommonCoreStandard)
                .Include(w => w.Template)
                .Include(w => w.Exports)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (worksheet == null)
            {
                return NotFound();
            }
            ViewData["Title"] = $"Worksheet Details - {worksheet.Title}";
            return View(worksheet);
        }

        // GET: Admin/Worksheets/Create
        public IActionResult Create()
        {
            ViewData["Title"] = "Create New Worksheet";
            ViewData["Users"] = new SelectList(_context.Users, "Id", "Email");
            ViewData["BloomLevels"] = new SelectList(_context.BloomLevels.OrderBy(b => b.Order), "Id", "Name");
            ViewData["CommonCoreStandards"] = new SelectList(_context.CommonCoreStandards.OrderBy(s => s.Code), "Id", "Code");
            ViewData["AcademicStandards"] = new SelectList(_context.AcademicStandards.OrderBy(s => s.GleCode), "Id", "GleCode");
            ViewData["Templates"] = new SelectList(_context.WorksheetTemplates.OrderBy(t => t.Name), "Id", "Name");

            // Add worksheet types and difficulty levels from configuration
            ViewData["WorksheetTypes"] = _config.SupportedWorksheetTypes
                .Distinct()
                .Select(t => new SelectListItem
                {
                    Value = t,
                    Text = FormatWorksheetTypeName(t)
                }).ToList();

            ViewData["DifficultyLevels"] = _config.SupportedDifficultyLevels
                .Distinct()
                .Select(d => new SelectListItem
                {
                    Value = d,
                    Text = FormatDifficultyLevel(d)
                }).ToList();

            return View();
        }

        // POST: Admin/Worksheets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,UserId,CommonCoreStandardId,AcademicStandardId,BloomLevelId,TemplateId,ContentMarkdown,RenderedHtml,SourceText,WorksheetType,DifficultyLevel,AccessibilityOptions,Tags,IsPublic,IsFavorite,LlmModel,GenerationPrompt,GenerationCost,GenerationTime,TokensUsed,ConfidenceScore,Warnings,QuestionCount,HasAnswerKey,EstimatedDurationMinutes")] Worksheet worksheet)
        {
            if (ModelState.IsValid)
            {
                // Validate JSON formats
                if (!string.IsNullOrEmpty(worksheet.AccessibilityOptions) && !IsValidJson(worksheet.AccessibilityOptions))
                {
                    ModelState.AddModelError("AccessibilityOptions", "Invalid JSON format for Accessibility Options.");
                }
                if (!string.IsNullOrEmpty(worksheet.Warnings) && !IsValidJson(worksheet.Warnings))
                {
                    ModelState.AddModelError("Warnings", "Invalid JSON format for Warnings.");
                }

                if (!ModelState.IsValid)
                {
                    ViewData["Title"] = "Create New Worksheet";
                    PopulateDropDowns(worksheet);
                    return View(worksheet);
                }

                worksheet.CreatedAt = DateTime.UtcNow;
                worksheet.UpdatedAt = DateTime.UtcNow;
                _context.Add(worksheet);
                await _context.SaveChangesAsync();
                SetSuccessMessage($"Worksheet '{worksheet.Title}' has been created successfully.");
                return RedirectToAction(nameof(Index));
            }
            ViewData["Title"] = "Create New Worksheet";
            PopulateDropDowns(worksheet);
            return View(worksheet);
        }

        // GET: Admin/Worksheets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var worksheet = await _context.Worksheets.FindAsync(id);
            if (worksheet == null)
            {
                return NotFound();
            }

            ViewData["Title"] = $"Edit Worksheet - {worksheet.Title}";
            PopulateDropDowns(worksheet);
            return View(worksheet);
        }        // POST: Admin/Worksheets/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,UserId,CommonCoreStandardId,AcademicStandardId,BloomLevelId,TemplateId,ContentMarkdown,RenderedHtml,SourceText,WorksheetType,DifficultyLevel,AccessibilityOptions,Tags,IsPublic,IsFavorite,CreatedAt,ViewCount,DownloadCount,LastAccessedAt,LlmModel,GenerationPrompt,GenerationCost,GenerationTime,TokensUsed,ConfidenceScore,Warnings,QuestionCount,HasAnswerKey,EstimatedDurationMinutes")] Worksheet worksheet)
        {
            if (id != worksheet.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {                    // Validate JSON formats
                    if (!string.IsNullOrEmpty(worksheet.AccessibilityOptions) && !IsValidJson(worksheet.AccessibilityOptions))
                    {
                        ModelState.AddModelError("AccessibilityOptions", "Invalid JSON format for Accessibility Options.");
                    }
                    if (!string.IsNullOrEmpty(worksheet.Warnings) && !IsValidJson(worksheet.Warnings))
                    {
                        ModelState.AddModelError("Warnings", "Invalid JSON format for Warnings.");
                    }

                    if (!ModelState.IsValid)
                    {
                        ViewData["Title"] = $"Edit Worksheet - {worksheet.Title}";
                        PopulateDropDowns(worksheet);
                        return View(worksheet);
                    }

                    worksheet.UpdatedAt = DateTime.UtcNow;
                    _context.Update(worksheet);
                    await _context.SaveChangesAsync();
                    SetSuccessMessage($"Worksheet '{worksheet.Title}' has been updated successfully.");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!WorksheetExists(worksheet.Id))
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
            ViewData["Title"] = $"Edit Worksheet - {worksheet.Title}";
            PopulateDropDowns(worksheet);
            return View(worksheet);
        }

        // GET: Admin/Worksheets/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var worksheet = await _context.Worksheets
                .Include(w => w.User)
                .Include(w => w.BloomLevel)
                .Include(w => w.CommonCoreStandard)
                .Include(w => w.Template)
                .Include(w => w.Exports)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (worksheet == null)
            {
                return NotFound();
            }

            ViewData["Title"] = $"Delete Worksheet - {worksheet.Title}";
            return View(worksheet);
        }

        // POST: Admin/Worksheets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var worksheet = await _context.Worksheets
                .Include(w => w.Exports)
                .FirstOrDefaultAsync(w => w.Id == id);

            if (worksheet != null)
            {
                // Delete related exports first
                _context.WorksheetExports.RemoveRange(worksheet.Exports);
                _context.Worksheets.Remove(worksheet);
                await _context.SaveChangesAsync();
                SetSuccessMessage($"Worksheet '{worksheet.Title}' and its exports have been deleted successfully.");
            }

            return RedirectToAction(nameof(Index));
        }
        private void PopulateDropDowns(Worksheet worksheet)
        {
            ViewData["Users"] = new SelectList(_context.Users, "Id", "Email", worksheet.UserId);
            ViewData["BloomLevels"] = new SelectList(_context.BloomLevels.OrderBy(b => b.Order), "Id", "Name", worksheet.BloomLevelId);
            ViewData["CommonCoreStandards"] = new SelectList(_context.CommonCoreStandards.OrderBy(s => s.Code), "Id", "Code", worksheet.CommonCoreStandardId);
            ViewData["AcademicStandards"] = new SelectList(_context.AcademicStandards.OrderBy(s => s.GleCode), "Id", "GleCode", worksheet.AcademicStandardId);
            ViewData["Templates"] = new SelectList(_context.WorksheetTemplates.OrderBy(t => t.Name), "Id", "Name", worksheet.TemplateId);

            // Add worksheet types and difficulty levels from configuration
            ViewData["WorksheetTypes"] = _config.SupportedWorksheetTypes
                .Distinct()
                .Select(t => new SelectListItem
                {
                    Value = t,
                    Text = FormatWorksheetTypeName(t),
                    Selected = t == worksheet.WorksheetType
                }).ToList();

            ViewData["DifficultyLevels"] = _config.SupportedDifficultyLevels
                .Distinct()
                .Select(d => new SelectListItem
                {
                    Value = d,
                    Text = FormatDifficultyLevel(d),
                    Selected = d == worksheet.DifficultyLevel
                }).ToList();
        }
        private bool WorksheetExists(int id)
        {
            return _context.Worksheets.Any(e => e.Id == id);
        }// API endpoint for DataTables
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetWorksheetsData()
        {
            var worksheets = await _context.Worksheets
                .Include(w => w.User)
                .Include(w => w.BloomLevel)
                .Include(w => w.CommonCoreStandard)
                .Include(w => w.Template)
                .OrderByDescending(w => w.CreatedAt)
                .Select(w => new
                {
                    id = w.Id,
                    title = w.Title ?? string.Empty,
                    user = w.User != null ? w.User.Email : string.Empty,
                    bloomLevel = w.BloomLevel != null ? w.BloomLevel.Name : string.Empty,
                    commonCoreStandard = w.CommonCoreStandard != null ? w.CommonCoreStandard.Code : string.Empty,
                    template = w.Template != null ? w.Template.Name : string.Empty,
                    worksheetType = w.WorksheetType ?? string.Empty,
                    difficultyLevel = w.DifficultyLevel,
                    isPublic = w.IsPublic,
                    viewCount = w.ViewCount,
                    downloadCount = w.DownloadCount,
                    createdAt = w.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                    // LLM Generation metadata
                    llmModel = w.LlmModel ?? string.Empty,
                    generationCost = w.GenerationCost ?? 0,
                    tokensUsed = w.TokensUsed ?? 0,
                    confidenceScore = w.ConfidenceScore ?? 0,
                    questionCount = w.QuestionCount,
                    hasAnswerKey = w.HasAnswerKey,
                    estimatedDurationMinutes = w.EstimatedDurationMinutes
                })
                .ToListAsync();

            return Json(new { data = worksheets });
        }

        /// <summary>
        /// Format worksheet type name for display
        /// </summary>
        private static string FormatWorksheetTypeName(string type)
        {
            return type switch
            {
                "reading-comprehension" => "Reading Comprehension",
                "vocabulary" => "Vocabulary",
                "grammar" => "Grammar",
                "creative-writing" => "Creative Writing",
                "literary-analysis" => "Literary Analysis",
                "math-practice" => "Math Practice",
                "writing-prompt" => "Writing Prompt",
                "science-lab" => "Science Lab",
                "history-timeline" => "History Timeline",
                "assessment" => "Assessment",
                _ => type.Replace("-", " ").Replace("_", " ")
            };
        }

        /// <summary>
        /// Format difficulty level for display
        /// </summary>
        private static string FormatDifficultyLevel(string level)
        {
            return level switch
            {
                "simplified" => "Simplified (Below Grade Level)",
                "standard" => "Standard (Grade Level)",
                "advanced" => "Advanced (Above Grade Level)",
                _ => level
            };
        }
    }
}
