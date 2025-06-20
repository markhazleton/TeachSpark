using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TeachSpark.Web.Configuration;
using TeachSpark.Web.Data;
using TeachSpark.Web.Services.Interfaces;
using TeachSpark.Web.Services.Models;

namespace TeachSpark.Web.Controllers
{
    /// <summary>
    /// Controller for public worksheet generation functionality
    /// </summary>
    [Authorize]
    public class WorksheetGeneratorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly WorksheetGenerationConfiguration _config;
        private readonly IModelRegistryService _modelRegistry;
        private readonly ILlmService _llmService;
        private readonly ILogger<WorksheetGeneratorController> _logger;

        public WorksheetGeneratorController(
            ApplicationDbContext context,
            IOptions<WorksheetGenerationConfiguration> config,
            IModelRegistryService modelRegistry,
            ILlmService llmService,
            ILogger<WorksheetGeneratorController> logger)
        {
            _context = context;
            _config = config.Value;
            _modelRegistry = modelRegistry;
            _llmService = llmService;
            _logger = logger;
        }

        /// <summary>
        /// Main worksheet generator landing page
        /// </summary>
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Worksheet Generator";

            // Prepare data for dropdowns
            await PrepareViewData();

            return View();
        }

        /// <summary>
        /// Worksheet creation form
        /// </summary>
        public async Task<IActionResult> Create()
        {
            ViewData["Title"] = "Create New Worksheet";

            await PrepareViewData();

            var model = new WorksheetGenerationRequest
            {
                WorksheetType = "reading-comprehension",
                DifficultyLevel = "standard",
                MaxQuestions = _config.DefaultQuestionCount,
                IncludeAnswerKey = true
            };

            return View(model);
        }        /// <summary>
                 /// Process worksheet generation request
                 /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WorksheetGenerationRequest request)
        {
            ViewData["Title"] = "Create New Worksheet";

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState is invalid. Errors: {Errors}",
                    string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                await PrepareViewData();
                return View(request);
            }

            try
            {
                _logger.LogInformation("Starting worksheet generation for user {User}: {WorksheetType}, MaxQuestions: {MaxQuestions}",
                    User.Identity?.Name, request.WorksheetType, request.MaxQuestions);

                // Generate worksheet using LLM service
                var result = await _llmService.GenerateWorksheetContentAsync(request);

                _logger.LogInformation("LLM service returned: Success={Success}, HasData={HasData}, ErrorMessage={ErrorMessage}",
                    result.Success, result.Data != null, result.ErrorMessage);

                if (result.Success && result.Data != null)
                {
                    _logger.LogInformation("Generated worksheet data: Title={Title}, ContentLength={ContentLength}, QuestionCount={QuestionCount}",
                        result.Data.GeneratedTitle, result.Data.MarkdownContent?.Length ?? 0, result.Data.ExtractedQuestions?.Count ?? 0);

                    // Save the generated worksheet to the database
                    _logger.LogInformation("Attempting to save worksheet to database");
                    var worksheet = await SaveGeneratedWorksheetAsync(request, result.Data);

                    if (worksheet != null)
                    {
                        _logger.LogInformation("Worksheet saved successfully with ID: {WorksheetId}", worksheet.Id);

                        // Store the generated worksheet content in TempData for the display view
                        TempData["GeneratedWorksheet"] = System.Text.Json.JsonSerializer.Serialize(result.Data);
                        TempData["WorksheetId"] = worksheet.Id;
                        TempData["Success"] = "Worksheet generated and saved successfully!";

                        // Redirect to a display page
                        return RedirectToAction("Display", new { id = worksheet.Id });
                    }
                    else
                    {
                        _logger.LogWarning("Worksheet generation succeeded but saving to database failed");

                        // Generation succeeded but saving failed - still show the result
                        TempData["GeneratedWorksheet"] = System.Text.Json.JsonSerializer.Serialize(result.Data);
                        TempData["Warning"] = "Worksheet generated successfully but could not be saved. You can still copy and use the content.";
                        return RedirectToAction("Display");
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, $"Failed to generate worksheet: {result.ErrorMessage}");
                    _logger.LogWarning("Worksheet generation failed: {Error}", result.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating worksheet for user {User}. Error: {ErrorMessage}",
                    User.Identity?.Name, ex.Message);
                ModelState.AddModelError(string.Empty, "An unexpected error occurred while generating the worksheet. Please try again.");
            }

            await PrepareViewData();
            return View(request);
        }/// <summary>
         /// Display generated worksheet
         /// </summary>
        public async Task<IActionResult> Display(int? id)
        {
            ViewData["Title"] = "Generated Worksheet";

            // First try to load from database if ID is provided
            if (id.HasValue)
            {
                try
                {                    // Get the actual user ID (not email)
                    var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                    var worksheet = await _context.Worksheets
                        .Include(w => w.CommonCoreStandard)
                        .Include(w => w.BloomLevel)
                        .Include(w => w.Template)
                        .FirstOrDefaultAsync(w => w.Id == id.Value && w.UserId == userIdClaim);

                    if (worksheet != null)
                    {
                        // Increment view count
                        worksheet.ViewCount++;
                        worksheet.LastAccessedAt = DateTime.UtcNow;
                        await _context.SaveChangesAsync();

                        // Convert database entity to display model
                        var content = ConvertWorksheetToContentResult(worksheet);
                        ViewData["Title"] = worksheet.Title;
                        ViewData["WorksheetId"] = worksheet.Id;
                        return View(content);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to load worksheet {Id} from database", id.Value);
                    TempData["Error"] = "Failed to load the requested worksheet.";
                }
            }

            // Fallback to TempData (for immediate display after generation)
            if (TempData["GeneratedWorksheet"] is string worksheetJson)
            {
                try
                {
                    var worksheet = System.Text.Json.JsonSerializer.Deserialize<WorksheetContentResult>(worksheetJson);
                    if (worksheet != null)
                    {
                        return View(worksheet);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to deserialize worksheet data");
                    TempData["Error"] = "Failed to load worksheet data.";
                }
            }

            TempData["Error"] = "No worksheet data found. Please generate a worksheet first.";
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Preview a worksheet template
        /// </summary>
        public async Task<IActionResult> PreviewTemplate(int id)
        {
            var template = await _context.WorksheetTemplates
                .FirstOrDefaultAsync(t => t.Id == id && t.IsPublic);

            if (template == null)
            {
                return NotFound();
            }

            ViewData["Title"] = $"Preview Template - {template.Name}";
            return View(template);
        }

        /// <summary>
        /// Get worksheet types for AJAX calls
        /// </summary>
        [HttpGet]
        public IActionResult GetWorksheetTypes()
        {
            var types = _config.SupportedWorksheetTypes.Select(t => new
            {
                value = t,
                text = FormatWorksheetTypeName(t)
            });

            return Json(types);
        }

        /// <summary>
        /// Get available templates for a specific worksheet type
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetTemplatesForType(string worksheetType)
        {
            var templates = await _context.WorksheetTemplates
                .Where(t => t.TemplateType == worksheetType && t.IsPublic)
                .Select(t => new
                {
                    value = t.Id,
                    text = t.Name,
                    description = t.Description
                })
                .ToListAsync();

            return Json(templates);
        }

        /// <summary>
        /// Get Common Core standards for a specific grade
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetStandardsForGrade(string grade = "8")
        {
            var standards = await _context.CommonCoreStandards
                .Where(s => s.Grade == grade)
                .Select(s => new
                {
                    value = s.Id,
                    text = $"{s.Code} - {s.Description}",
                    domain = s.Domain
                })
                .ToListAsync();

            return Json(standards);
        }

        /// <summary>
        /// Prepare view data for dropdowns and form elements
        /// </summary>
        private async Task PrepareViewData()
        {
            // Worksheet types
            ViewBag.WorksheetTypes = _config.SupportedWorksheetTypes.Select(t => new SelectListItem
            {
                Value = t,
                Text = FormatWorksheetTypeName(t)
            }).ToList();

            // Difficulty levels
            ViewBag.DifficultyLevels = _config.SupportedDifficultyLevels.Select(d => new SelectListItem
            {
                Value = d,
                Text = FormatDifficultyLevel(d)
            }).ToList();

            // Common Core Standards (8th grade ELA)
            ViewBag.CommonCoreStandards = await _context.CommonCoreStandards
                .Where(s => s.Grade == "8")
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = $"{s.Code} - {s.Description}"
                })
                .ToListAsync();

            // Bloom's Taxonomy Levels
            ViewBag.BloomLevels = await _context.BloomLevels
                .Select(b => new SelectListItem
                {
                    Value = b.Id.ToString(),
                    Text = $"{b.Name} - {b.Description}"
                })
                .ToListAsync();

            // Available templates
            ViewBag.Templates = await _context.WorksheetTemplates
                .Where(t => t.IsPublic)
                .Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.Name
                })
                .ToListAsync();

            // Configuration values
            ViewBag.MaxQuestions = _config.MaxQuestionCount;
            ViewBag.DefaultQuestions = _config.DefaultQuestionCount;

            // Available AI models for worksheet generation
            try
            {
                var modelsResult = await _modelRegistry.GetEducationRecommendedModelsAsync();
                if (modelsResult.Success && modelsResult.Data != null)
                {
                    ViewBag.AvailableModels = modelsResult.Data.Select(m => new SelectListItem
                    {
                        Value = m.Id,
                        Text = $"{m.Name} - ${m.CostPer1kInputTokens:F4}/1k tokens"
                    }).ToList();
                }
                else
                {
                    ViewBag.AvailableModels = new List<SelectListItem>
                    {
                        new SelectListItem { Value = "gpt-4o-mini", Text = "GPT-4o Mini (Default)" }
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load available models, using default");
                ViewBag.AvailableModels = new List<SelectListItem>
                {
                    new SelectListItem { Value = "gpt-4o-mini", Text = "GPT-4o Mini (Default)" }
                };
            }
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
        }        /// <summary>
                 /// Save generated worksheet to the database
                 /// </summary>
        private async Task<Data.Entities.Worksheet?> SaveGeneratedWorksheetAsync(
            WorksheetGenerationRequest request,
            WorksheetContentResult content)
        {
            try
            {
                _logger.LogInformation("Starting SaveGeneratedWorksheetAsync");

                // Get the actual user ID (not email)
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    _logger.LogWarning("Cannot save worksheet: User ID claim not found");
                    return null;
                }

                _logger.LogInformation("User authenticated with ID: {UserId}, Email: {Email}",
                    userIdClaim, User.Identity?.Name);

                // Serialize warnings as JSON
                var warningsJson = content.Warnings.Any() ?
                    System.Text.Json.JsonSerializer.Serialize(content.Warnings) : null;

                // Serialize accessibility options and tags as JSON
                var accessibilityOptionsJson = request.AccessibilityOptions.Any() ?
                    System.Text.Json.JsonSerializer.Serialize(request.AccessibilityOptions) : null;
                var tagsJson = request.Tags.Any() ?
                    string.Join(",", request.Tags) : null;

                _logger.LogInformation("Creating worksheet entity with Title: {Title}, Type: {Type}, Content Length: {ContentLength}",
                    content.GeneratedTitle, request.WorksheetType, content.MarkdownContent?.Length ?? 0);

                var worksheet = new Data.Entities.Worksheet
                {
                    Title = content.GeneratedTitle,
                    Description = content.GeneratedDescription,
                    UserId = userIdClaim, // Use the actual user ID, not email
                    CommonCoreStandardId = request.CommonCoreStandardId,
                    BloomLevelId = request.BloomLevelId,
                    TemplateId = request.TemplateId,
                    ContentMarkdown = content.MarkdownContent ?? string.Empty,
                    RenderedHtml = content.RenderedHtml,
                    SourceText = request.SourceText,
                    WorksheetType = request.WorksheetType,
                    DifficultyLevel = request.DifficultyLevel,
                    AccessibilityOptions = accessibilityOptionsJson,
                    Tags = tagsJson,
                    IsPublic = false, // Default to private
                    IsFavorite = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    ViewCount = 0,
                    DownloadCount = 0,

                    // Generation metadata
                    LlmModel = content.LlmModel,
                    GenerationPrompt = !string.IsNullOrEmpty(content.GenerationPrompt) ? content.GenerationPrompt : "AI-generated content",
                    GenerationCost = content.GenerationCost,
                    GenerationTime = content.GenerationTime,
                    TokensUsed = content.TokensUsed,
                    ConfidenceScore = content.ConfidenceScore,
                    Warnings = warningsJson,
                    QuestionCount = content.ExtractedQuestions.Count,
                    HasAnswerKey = content.HasAnswerKey,
                    EstimatedDurationMinutes = content.EstimatedDurationMinutes
                };

                _logger.LogInformation("Adding worksheet to context");
                _context.Worksheets.Add(worksheet);

                _logger.LogInformation("Calling SaveChangesAsync");
                var changeCount = await _context.SaveChangesAsync();

                _logger.LogInformation("SaveChangesAsync completed, changes saved: {ChangeCount}, Worksheet ID: {WorksheetId}",
                    changeCount, worksheet.Id);

                _logger.LogInformation("Successfully saved generated worksheet {WorksheetId} for user {UserId}",
                    worksheet.Id, userIdClaim);

                return worksheet;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save generated worksheet to database. Error: {ErrorMessage}", ex.Message);
                _logger.LogError("Stack trace: {StackTrace}", ex.StackTrace);
                return null;
            }
        }

        /// <summary>
        /// Convert database worksheet entity to display content result
        /// </summary>
        private WorksheetContentResult ConvertWorksheetToContentResult(Data.Entities.Worksheet worksheet)
        {
            // Deserialize warnings from JSON
            var warnings = new List<string>();
            if (!string.IsNullOrEmpty(worksheet.Warnings))
            {
                try
                {
                    warnings = System.Text.Json.JsonSerializer.Deserialize<List<string>>(worksheet.Warnings) ?? new List<string>();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to deserialize warnings for worksheet {Id}", worksheet.Id);
                    warnings = new List<string> { "Warning data could not be loaded" };
                }
            }

            return new WorksheetContentResult
            {
                MarkdownContent = worksheet.ContentMarkdown,
                RenderedHtml = worksheet.RenderedHtml ?? string.Empty,
                GeneratedTitle = worksheet.Title,
                GeneratedDescription = worksheet.Description ?? string.Empty,
                LlmModel = worksheet.LlmModel ?? "Unknown",
                GenerationPrompt = worksheet.GenerationPrompt ?? "Not available",
                GenerationCost = worksheet.GenerationCost ?? 0,
                GenerationTime = worksheet.GenerationTime ?? TimeSpan.Zero,
                TokensUsed = worksheet.TokensUsed ?? 0,
                ConfidenceScore = worksheet.ConfidenceScore ?? 0,
                Warnings = warnings,
                ExtractedQuestions = ExtractQuestionsFromMarkdown(worksheet.ContentMarkdown),
                HasAnswerKey = worksheet.HasAnswerKey,
                EstimatedDurationMinutes = worksheet.EstimatedDurationMinutes
            };
        }

        /// <summary>
        /// Extract questions from markdown content for display
        /// </summary>
        private List<string> ExtractQuestionsFromMarkdown(string markdown)
        {
            if (string.IsNullOrEmpty(markdown))
                return new List<string>();

            var questions = new List<string>();
            var lines = markdown.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                // Look for numbered questions or lines ending with question marks
                if (System.Text.RegularExpressions.Regex.IsMatch(trimmedLine, @"^\d+\.?\s+") || trimmedLine.EndsWith('?'))
                {
                    questions.Add(trimmedLine);
                }
            }

            return questions;
        }
    }
}
