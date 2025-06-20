using Microsoft.AspNetCore.Mvc;
using TeachSpark.Web.Services.Interfaces;
using TeachSpark.Web.Services.Models;

namespace TeachSpark.Web.Controllers
{
    /// <summary>
    /// API Controller for managing AI models and model selection
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ModelsController : ControllerBase
    {
        private readonly IModelRegistryService _modelRegistry;
        private readonly ILogger<ModelsController> _logger;

        public ModelsController(
            IModelRegistryService modelRegistry,
            ILogger<ModelsController> logger)
        {
            _modelRegistry = modelRegistry;
            _logger = logger;
        }        /// <summary>
                 /// Get all available models
                 /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAvailableModels([FromQuery] bool forceRefresh = false)
        {
            try
            {
                var result = await _modelRegistry.GetAvailableModelsAsync(forceRefresh);

                if (result.Success)
                {
                    return Ok(new { success = true, data = result.Data });
                }

                return Ok(new { success = false, message = result.ErrorMessage });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get available models");
                return Ok(new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get models recommended for educational content
        /// </summary>
        [HttpGet("education")]
        public async Task<IActionResult> GetEducationModels()
        {
            try
            {
                var result = await _modelRegistry.GetEducationRecommendedModelsAsync();

                if (result.Success)
                {
                    return Ok(result.Data);
                }

                return BadRequest(result.ErrorMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get education models");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get model recommendation based on criteria
        /// </summary>
        [HttpPost("recommend")]
        public async Task<IActionResult> RecommendModel([FromBody] ModelSelectionCriteria criteria)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _modelRegistry.RecommendModelAsync(criteria);

                if (result.Success)
                {
                    return Ok(result.Data);
                }

                return BadRequest(result.ErrorMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to recommend model");
                return StatusCode(500, "Internal server error");
            }
        }        /// <summary>
                 /// Get model recommendation based on worksheet type (GET method for easy frontend use)
                 /// </summary>
        [HttpGet("recommend")]
        public async Task<IActionResult> RecommendModelByWorksheetType([FromQuery] string worksheetType, [FromQuery] string difficulty = "standard")
        {
            try
            {
                var criteria = new ModelSelectionCriteria
                {
                    WorksheetType = worksheetType ?? string.Empty,
                    DifficultyLevel = difficulty,
                    EstimatedTokens = 1500, // Default estimate for worksheet generation
                    MaxCostPerRequest = 1.00m, // $1 max per request
                    RequireStructuredOutput = true,
                    PrioritizeQuality = true,
                    PrioritizeSpeed = false
                };

                var result = await _modelRegistry.RecommendModelAsync(criteria); if (result.Success)
                {
                    // Return the recommended model as the first item in a list for frontend compatibility
                    var recommendations = new List<object>
                    {
                        new
                        {
                            id = result.Data!.RecommendedModel.Id,
                            name = result.Data.RecommendedModel.Name,
                            description = result.Data.RecommendedModel.Description,
                            reason = result.Data.Reason,
                            estimatedCost = result.Data.EstimatedCost,
                            maxTokens = result.Data.RecommendedModel.MaxTokens,
                            costPer1kInputTokens = result.Data.RecommendedModel.CostPer1kInputTokens,
                            costPer1kOutputTokens = result.Data.RecommendedModel.CostPer1kOutputTokens,
                            strengths = result.Data.RecommendedModel.Strengths,
                            supportsStructuredOutput = result.Data.RecommendedModel.SupportsStructuredOutput
                        }
                    };

                    return Ok(new { success = true, data = recommendations });
                }

                return Ok(new { success = false, message = result.ErrorMessage });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to recommend model for worksheet type {WorksheetType}", worksheetType);
                return Ok(new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get specific model by ID
        /// </summary>
        [HttpGet("{modelId}")]
        public async Task<IActionResult> GetModel(string modelId)
        {
            try
            {
                var result = await _modelRegistry.GetModelByIdAsync(modelId);

                if (result.Success)
                {
                    return Ok(result.Data);
                }

                return NotFound(result.ErrorMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get model {ModelId}", modelId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Estimate cost for a model and token count
        /// </summary>
        [HttpPost("{modelId}/estimate-cost")]
        public async Task<IActionResult> EstimateCost(string modelId, [FromBody] int estimatedTokens)
        {
            try
            {
                var result = await _modelRegistry.EstimateCostAsync(modelId, estimatedTokens);

                if (result.Success)
                {
                    return Ok(new { ModelId = modelId, EstimatedTokens = estimatedTokens, EstimatedCost = result.Data });
                }

                return BadRequest(result.ErrorMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to estimate cost for model {ModelId}", modelId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Refresh the model registry from OpenAI API
        /// </summary>
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshModels()
        {
            try
            {
                var result = await _modelRegistry.RefreshModelRegistryAsync();

                if (result.Success)
                {
                    return Ok(new { Message = "Model registry refreshed successfully" });
                }

                return BadRequest(result.ErrorMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to refresh model registry");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Estimate cost based on request details (simplified endpoint for frontend)
        /// </summary>
        [HttpPost("estimate-cost")]
        public async Task<IActionResult> EstimateCostSimple([FromBody] CostEstimationRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Get the model details first
                var modelResult = await _modelRegistry.GetModelByIdAsync(request.ModelId);
                if (!modelResult.Success || modelResult.Data == null)
                {
                    return BadRequest($"Model {request.ModelId} not found");
                }

                var model = modelResult.Data;

                // Calculate estimated cost
                var inputCost = (decimal)request.InputTokens / 1000 * model.CostPer1kInputTokens;
                var outputCost = (decimal)request.OutputTokens / 1000 * model.CostPer1kOutputTokens;
                var totalCost = inputCost + outputCost;

                var response = new
                {
                    success = true,
                    data = new
                    {
                        modelId = request.ModelId,
                        inputTokens = request.InputTokens,
                        outputTokens = request.OutputTokens,
                        estimatedCost = totalCost,
                        breakdown = new
                        {
                            inputCost = inputCost,
                            outputCost = outputCost,
                            inputRate = model.CostPer1kInputTokens,
                            outputRate = model.CostPer1kOutputTokens
                        }
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to estimate cost for request");
                return Ok(new { success = false, message = "Unable to estimate cost" });
            }
        }

        /// <summary>
        /// Cost estimation request model
        /// </summary>
        public class CostEstimationRequest
        {
            public string ModelId { get; set; } = string.Empty;
            public int InputTokens { get; set; }
            public int OutputTokens { get; set; }
            public string? WorksheetType { get; set; }
        }
    }
}
