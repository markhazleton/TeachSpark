using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TeachSpark.Web.Configuration;
using TeachSpark.Web.Data;
using TeachSpark.Web.Services.Models;
using TeachSpark.Web.Services.Interfaces;

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
        }

        /// <summary>
        /// Process worksheet generation request
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WorksheetGenerationRequest request)
        {
            ViewData["Title"] = "Create New Worksheet";

            if (!ModelState.IsValid)
            {
                await PrepareViewData();
                return View(request);
            }

            try
            {
                _logger.LogInformation("Generating worksheet for user {User}: {WorksheetType}", 
                    User.Identity?.Name, request.WorksheetType);

                // Generate worksheet using LLM service
                var result = await _llmService.GenerateWorksheetContentAsync(request);

                if (result.Success)
                {
                    // Store the generated worksheet content in TempData for the display view
                    TempData["GeneratedWorksheet"] = System.Text.Json.JsonSerializer.Serialize(result.Data);
                    TempData["Success"] = "Worksheet generated successfully!";
                    
                    // Redirect to a display page
                    return RedirectToAction("Display");
                }
                else
                {
                    ModelState.AddModelError("", $"Failed to generate worksheet: {result.ErrorMessage}");
                    _logger.LogWarning("Worksheet generation failed: {Error}", result.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating worksheet for user {User}", User.Identity?.Name);
                ModelState.AddModelError("", "An unexpected error occurred while generating the worksheet. Please try again.");
            }

            await PrepareViewData();
            return View(request);
        }

        /// <summary>
        /// Display generated worksheet
        /// </summary>
        public IActionResult Display()
        {
            ViewData["Title"] = "Generated Worksheet";

            if (TempData["GeneratedWorksheet"] is string worksheetJson)
            {
                try
                {
                    var worksheet = System.Text.Json.JsonSerializer.Deserialize<WorksheetContentResult>(worksheetJson);
                    return View(worksheet);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to deserialize worksheet data");
                    TempData["Error"] = "Failed to load worksheet data.";
                }
            }
            else
            {
                TempData["Error"] = "No worksheet data found. Please generate a worksheet first.";
            }

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
        }
    }
}
