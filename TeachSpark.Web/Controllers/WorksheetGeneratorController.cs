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
    public class WorksheetGeneratorController(
        ApplicationDbContext context,
        IOptions<WorksheetGenerationConfiguration> config,
        IModelRegistryService modelRegistry,
        ILlmService llmService,
        ILogger<WorksheetGeneratorController> logger) : Controller
    {
        private readonly WorksheetGenerationConfiguration _config = config.Value;

        /// <summary>
        /// Main worksheet generator landing page
        /// </summary>
        public IActionResult Index()
        {
            ViewData["Title"] = "Worksheet Generator";
            return View();
        }        /// <summary>
                 /// Worksheet creation form
                 /// </summary>
        public async Task<IActionResult> Create()
        {
            ViewData["Title"] = "Create New Worksheet";

            // Get the first available template to set as default
            var firstTemplate = await context.WorksheetTemplates
                .Where(t => t.IsPublic)
                .OrderBy(t => t.Name)
                .FirstOrDefaultAsync();

            var model = new WorksheetGenerationRequest
            {
                TemplateId = firstTemplate?.Id ?? 1, // Default to first template or ID 1
                DifficultyLevel = "standard",
                MaxQuestions = _config.DefaultQuestionCount,
                IncludeAnswerKey = true
            };

            await PopulateDropdownLists(model);

            return View(model);
        }/// <summary>
         /// Process worksheet generation request
         /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WorksheetGenerationRequest request)
        {
            ViewData["Title"] = "Create New Worksheet"; if (!ModelState.IsValid)
            {
                logger.LogWarning("ModelState is invalid. Errors: {Errors}",
                    string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                await PopulateDropdownLists(request);
                return View(request);
            }
            try
            {
                // Get the template to determine worksheet type
                request.Template = await context.WorksheetTemplates
                    .FirstOrDefaultAsync(t => t.Id == request.TemplateId) ?? new();

                // Get user information for logging
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var userEmail = User.Identity?.Name;

                // Generate worksheet using LLM service
                var result = await llmService.GenerateWorksheetContentAsync(request, userId, userEmail);

                logger.LogInformation("LLM service returned: Success={Success}, HasData={HasData}, ErrorMessage={ErrorMessage}",
                    result.Success, result.Data != null, result.ErrorMessage);

                if (result.Success && result.Data != null)
                {
                    logger.LogInformation("Generated worksheet data: Title={Title}, ContentLength={ContentLength}, QuestionCount={QuestionCount}",
                        result.Data.GeneratedTitle, result.Data.MarkdownContent?.Length ?? 0, result.Data.ExtractedQuestions?.Count ?? 0);

                    // Save the generated worksheet to the database
                    logger.LogInformation("Attempting to save worksheet to database");
                    var worksheet = await SaveGeneratedWorksheetAsync(request, result.Data);

                    if (worksheet != null)
                    {
                        logger.LogInformation("Worksheet saved successfully with ID: {WorksheetId}", worksheet.Id);

                        // Store the generated worksheet content in TempData for the display view
                        TempData["GeneratedWorksheet"] = System.Text.Json.JsonSerializer.Serialize(result.Data);
                        TempData["WorksheetId"] = worksheet.Id;
                        TempData["Success"] = "Worksheet generated and saved successfully!";

                        // Small delay to help with CloudFlare edge caching after long generation
                        await Task.Delay(500);

                        // Redirect to a display page
                        return RedirectToAction("Display", new { id = worksheet.Id });
                    }
                    else
                    {
                        logger.LogWarning("Worksheet generation succeeded but saving to database failed");

                        // Generation succeeded but saving failed - still show the result
                        TempData["GeneratedWorksheet"] = System.Text.Json.JsonSerializer.Serialize(result.Data);
                        TempData["Warning"] = "Worksheet generated successfully but could not be saved. You can still copy and use the content.";
                        return RedirectToAction("Display");
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, $"Failed to generate worksheet: {result.ErrorMessage}");
                    logger.LogWarning("Worksheet generation failed: {Error}", result.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error generating worksheet for user {User}. Error: {ErrorMessage}",
                    User.Identity?.Name, ex.Message);
                ModelState.AddModelError(string.Empty, "An unexpected error occurred while generating the worksheet. Please try again.");
            }

            await PopulateDropdownLists(request);
            return View(request);
        }        /// <summary>
                 /// Display generated worksheet
                 /// </summary>
        public async Task<IActionResult> Display(int? id)
        {
            ViewData["Title"] = "Generated Worksheet";
            logger.LogInformation("Display action called with ID: {Id}", id);

            // Add cache control headers to help with CloudFlare edge caching
            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";

            // First try to load from database if ID is provided
            if (id.HasValue)
            {
                try
                {
                    // Get the actual user ID (not email)
                    var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                    var userEmailClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

                    logger.LogInformation("Attempting to load worksheet {Id} for user {UserId} (Email: {Email})",
                        id.Value, userIdClaim, userEmailClaim);

                    // First check if worksheet exists at all
                    var worksheetExists = await context.Worksheets
                        .AnyAsync(w => w.Id == id.Value);

                    if (!worksheetExists)
                    {
                        logger.LogWarning("Worksheet {Id} does not exist in database", id.Value);
                        TempData["Error"] = $"Worksheet {id.Value} not found.";
                        return RedirectToAction("Index");
                    }

                    // Get worksheet with user info for debugging
                    var worksheetInfo = await context.Worksheets
                        .Select(w => new { w.Id, w.UserId, w.Title })
                        .FirstOrDefaultAsync(w => w.Id == id.Value);

                    if (worksheetInfo != null)
                    {
                        logger.LogInformation("Found worksheet {Id} with UserId: {WorksheetUserId}, Current User: {CurrentUserId}",
                            worksheetInfo.Id, worksheetInfo.UserId, userIdClaim);
                    }

                    var worksheet = await context.Worksheets
                        .Include(w => w.CommonCoreStandard)
                        .Include(w => w.BloomLevel)
                        .Include(w => w.Template)
                        .FirstOrDefaultAsync(w => w.Id == id.Value && w.UserId == userIdClaim);

                    if (worksheet != null)
                    {
                        logger.LogInformation("Successfully loaded worksheet {Id} for user {UserId}", worksheet.Id, userIdClaim);

                        // Increment view count
                        worksheet.ViewCount++;
                        worksheet.LastAccessedAt = DateTime.UtcNow;
                        await context.SaveChangesAsync();

                        // Convert database entity to display model
                        var content = ConvertWorksheetToContentResult(worksheet);
                        ViewData["Title"] = worksheet.Title;
                        ViewData["WorksheetId"] = worksheet.Id;
                        return View(content);
                    }
                    else
                    {
                        logger.LogWarning("Worksheet {Id} not found for user {UserId} - access denied or not found",
                            id.Value, userIdClaim);
                        TempData["Error"] = "Worksheet not found or you don't have permission to view it.";
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to load worksheet {Id} from database", id.Value);
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
                    logger.LogError(ex, "Failed to deserialize worksheet data");
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
            var template = await context.WorksheetTemplates
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
            var templates = await context.WorksheetTemplates
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
            var standards = await context.CommonCoreStandards
                .Where(s => s.Grade == grade)
                .Select(s => new
                {
                    value = s.Id,
                    text = $"{s.Code} - {s.Description}",
                    domain = s.Domain
                })
                .ToListAsync();

            return Json(standards);
        }        /// <summary>
                 /// Populate dropdown lists in the view model
                 /// </summary>
        private async Task PopulateDropdownLists(WorksheetGenerationRequest model)
        {
            // Difficulty levels
            model.DifficultyLevelOptions = [.. _config.SupportedDifficultyLevels
                .Distinct()
                .Select(d => new SelectListItem
                {
                    Value = d,
                    Text = FormatDifficultyLevel(d),
                    Selected = d == model.DifficultyLevel
                })];

            // Common Core Standards (8th grade ELA)
            model.CommonCoreStandardOptions = await context.CommonCoreStandards
                .Where(s => s.Grade == "8")
                .OrderBy(s => s.Code)
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = $"{s.Code} - {s.Description}",
                    Selected = s.Id == model.CommonCoreStandardId
                })
                .Distinct()
                .ToListAsync();

            // Add empty option at the beginning
            model.CommonCoreStandardOptions.Insert(0, new SelectListItem
            {
                Value = "",
                Text = "No specific standard (optional)",
                Selected = !model.CommonCoreStandardId.HasValue
            });

            // Bloom's Taxonomy Levels
            model.BloomLevelOptions = await context.BloomLevels
                .OrderBy(b => b.Order)
                .Select(b => new SelectListItem
                {
                    Value = b.Id.ToString(),
                    Text = $"{b.Name} - {b.Description}",
                    Selected = b.Id == model.BloomLevelId
                })
                .Distinct()
                .ToListAsync();

            // Add empty option at the beginning
            model.BloomLevelOptions.Insert(0, new SelectListItem
            {
                Value = "",
                Text = "Mixed levels (recommended)",
                Selected = !model.BloomLevelId.HasValue
            });            // Available templates
            model.TemplateOptions = await context.WorksheetTemplates
                .Where(t => t.IsPublic)
                .OrderBy(t => t.Name)
                .Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.Name,
                    Selected = t.Id == model.TemplateId
                })
                .Distinct()
                .ToListAsync();

            // Available AI models for worksheet generation
            try
            {
                var modelsResult = await modelRegistry.GetEducationRecommendedModelsAsync();
                if (modelsResult.Success && modelsResult.Data != null)
                {
                    model.AvailableModelOptions = [.. modelsResult.Data.Select(m => new SelectListItem
                    {
                        Value = m.Id,
                        Text = $"{m.Name} - ${m.CostPer1kInputTokens:F4}/1k tokens",
                        Selected = m.Id == model.PreferredLlmModel
                    })];
                }
                else
                {
                    model.AvailableModelOptions = new List<SelectListItem>
                    {
                        new() {
                            Value = "gpt-4o-mini",
                            Text = "GPT-4o Mini (Default)",
                            Selected = model.PreferredLlmModel == "gpt-4o-mini" || string.IsNullOrEmpty(model.PreferredLlmModel)
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to load available models, using default");
                model.AvailableModelOptions = new List<SelectListItem>
                {
                    new SelectListItem {
                        Value = "gpt-4o-mini",
                        Text = "GPT-4o Mini (Default)",
                        Selected = model.PreferredLlmModel == "gpt-4o-mini" || string.IsNullOrEmpty(model.PreferredLlmModel)
                    }
                };
            }

            // Set configuration values in ViewBag (these are not dropdowns)
            ViewBag.MaxQuestions = _config.MaxQuestionCount;
            ViewBag.DefaultQuestions = _config.DefaultQuestionCount;
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
        }        /// <summary>        /// <summary>
                 /// Save generated worksheet to the database
                 /// </summary>
        private async Task<Data.Entities.Worksheet?> SaveGeneratedWorksheetAsync(
            WorksheetGenerationRequest request,
            WorksheetContentResult content)
        {
            try
            {
                logger.LogInformation("Starting SaveGeneratedWorksheetAsync");

                // Get the template to determine worksheet type
                var template = await context.WorksheetTemplates
                    .FirstOrDefaultAsync(t => t.Id == request.TemplateId);

                var worksheetType = template?.TemplateType ?? "unknown";

                // Get the actual user ID (not email)
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    logger.LogWarning("Cannot save worksheet: User ID claim not found");
                    return null;
                }

                logger.LogInformation("User authenticated with ID: {UserId}, Email: {Email}",
                    userIdClaim, User.Identity?.Name);

                // Serialize warnings as JSON
                var warningsJson = content.Warnings.Any() ?
                    System.Text.Json.JsonSerializer.Serialize(content.Warnings) : null;

                // Serialize accessibility options and tags as JSON
                var accessibilityOptionsJson = request.AccessibilityOptions.Any() ?
                    System.Text.Json.JsonSerializer.Serialize(request.AccessibilityOptions) : null;
                var tagsJson = request.Tags.Count != 0 ?
                    string.Join(",", request.Tags) : null; logger.LogInformation("Creating worksheet entity with Title: {Title}, Type: {Type}, Content Length: {ContentLength}",
                    content.GeneratedTitle, worksheetType, content.MarkdownContent?.Length ?? 0);

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
                    WorksheetType = worksheetType,
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

                logger.LogInformation("Adding worksheet to context");
                context.Worksheets.Add(worksheet);

                logger.LogInformation("Calling SaveChangesAsync");
                var changeCount = await context.SaveChangesAsync();

                logger.LogInformation("SaveChangesAsync completed, changes saved: {ChangeCount}, Worksheet ID: {WorksheetId}",
                    changeCount, worksheet.Id);

                logger.LogInformation("Successfully saved generated worksheet {WorksheetId} for user {UserId}",
                    worksheet.Id, userIdClaim);

                return worksheet;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to save generated worksheet to database. Error: {ErrorMessage}", ex.Message);
                logger.LogError("Stack trace: {StackTrace}", ex.StackTrace);
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
                    logger.LogWarning(ex, "Failed to deserialize warnings for worksheet {Id}", worksheet.Id);
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

        /// <summary>
        /// Get template information by ID for AJAX calls
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetTemplate(int id)
        {
            var template = await context.WorksheetTemplates
                .FirstOrDefaultAsync(t => t.Id == id && t.IsPublic);

            if (template == null)
            {
                return Json(new { success = false, message = "Template not found" });
            }

            return Json(new
            {
                success = true,
                data = new
                {
                    id = template.Id,
                    name = template.Name,
                    description = template.Description,
                    templateType = template.TemplateType
                }
            });
        }
    }
}
