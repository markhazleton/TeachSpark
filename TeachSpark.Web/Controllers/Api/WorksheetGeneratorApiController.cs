using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeachSpark.Web.Services.Interfaces;
using TeachSpark.Web.Services.Models;

namespace TeachSpark.Web.Controllers.Api
{    /// <summary>
     /// API controller for worksheet generator functionality
     /// </summary>
    [ApiController]
    [Route("api/worksheet-generator")]
    [Authorize]
    public class WorksheetGeneratorApiController : ControllerBase
    {
        private readonly IModelRegistryService _modelRegistry;
        private readonly ILogger<WorksheetGeneratorApiController> _logger;

        public WorksheetGeneratorApiController(
            IModelRegistryService modelRegistry,
            ILogger<WorksheetGeneratorApiController> logger)
        {
            _modelRegistry = modelRegistry;
            _logger = logger;
        }        /// <summary>
                 /// Get available AI models
                 /// </summary>
        [HttpGet("models")]
        public async Task<IActionResult> GetModels(CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _modelRegistry.GetEducationRecommendedModelsAsync(cancellationToken);

                if (result.Success)
                {
                    return Ok(new { success = true, data = result.Data });
                }

                return BadRequest(new { success = false, error = result.ErrorMessage });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get available models");
                return StatusCode(500, new { success = false, error = "Internal server error" });
            }
        }

        /// <summary>
        /// Get model recommendations for a specific worksheet type
        /// </summary>
        [HttpGet("models/recommend")]
        public async Task<IActionResult> GetModelRecommendations(
            [FromQuery] string worksheetType,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var criteria = new ModelSelectionCriteria
                {
                    WorksheetType = worksheetType,
                    PrioritizeQuality = true,
                    RequireStructuredOutput = false
                };

                var result = await _modelRegistry.RecommendModelAsync(criteria, cancellationToken);

                if (result.Success && result.Data != null)
                {
                    // Return as array for consistency with frontend expectations
                    var recommendations = new[] { result.Data.RecommendedModel };
                    return Ok(new { success = true, data = recommendations });
                }

                return BadRequest(new { success = false, error = result.ErrorMessage });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get model recommendations for worksheet type: {WorksheetType}", worksheetType);
                return StatusCode(500, new { success = false, error = "Internal server error" });
            }
        }

        /// <summary>
        /// Estimate cost for worksheet generation
        /// </summary>
        [HttpPost("models/estimate-cost")]
        public async Task<IActionResult> EstimateCost(
            [FromBody] CostEstimateRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(request.ModelId))
                {
                    return BadRequest(new { success = false, error = "Model ID is required" });
                }

                var modelsResult = await _modelRegistry.GetEducationRecommendedModelsAsync(cancellationToken);
                if (!modelsResult.Success || modelsResult.Data == null)
                {
                    return BadRequest(new { success = false, error = "Unable to retrieve model information" });
                }

                var model = modelsResult.Data.FirstOrDefault(m => m.Id == request.ModelId);
                if (model == null)
                {
                    return BadRequest(new { success = false, error = "Model not found" });
                }

                // Calculate estimated cost
                var inputCost = (decimal)request.InputTokens / 1000 * model.CostPer1kInputTokens;
                var outputCost = (decimal)request.OutputTokens / 1000 * model.CostPer1kOutputTokens;
                var totalCost = inputCost + outputCost;

                var response = new
                {
                    success = true,
                    data = new
                    {
                        estimatedCost = totalCost,
                        inputTokens = request.InputTokens,
                        outputTokens = request.OutputTokens,
                        inputCost = inputCost,
                        outputCost = outputCost,
                        modelName = model.Name
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to estimate cost for model: {ModelId}", request.ModelId);
                return StatusCode(500, new { success = false, error = "Internal server error" });
            }
        }
    }

    /// <summary>
    /// Request model for cost estimation
    /// </summary>
    public class CostEstimateRequest
    {
        public string ModelId { get; set; } = string.Empty;
        public int InputTokens { get; set; }
        public int OutputTokens { get; set; }
        public string WorksheetType { get; set; } = string.Empty;
    }
}
