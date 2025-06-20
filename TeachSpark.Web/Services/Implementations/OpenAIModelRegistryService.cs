using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using OpenAI;
using TeachSpark.Web.Services.Interfaces;
using TeachSpark.Web.Services.Models;

namespace TeachSpark.Web.Services.Implementations
{
    /// <summary>
    /// Service for managing available OpenAI models and selecting the best model for specific tasks
    /// </summary>
    public class OpenAIModelRegistryService : IModelRegistryService
    {
        private readonly Configuration.LlmConfiguration _config;
        private readonly IMemoryCache _cache;
        private readonly OpenAIClient _openAIClient;
        private readonly ILogger<OpenAIModelRegistryService> _logger;
        private readonly List<OpenAIModel> _staticModels;
        private const string MODELS_CACHE_KEY = "openai_models";
        private const int CACHE_EXPIRATION_HOURS = 24;

        public OpenAIModelRegistryService(
            IOptions<Configuration.LlmConfiguration> config,
            IMemoryCache cache,
            OpenAIClient openAIClient,
            ILogger<OpenAIModelRegistryService> logger)
        {
            _config = config.Value;
            _cache = cache;
            _openAIClient = openAIClient;
            _logger = logger;
            _staticModels = InitializeStaticModels();
        }

        /// <summary>
        /// Get all available models with option to force refresh from OpenAI API
        /// </summary>
        public async Task<ServiceResult<List<OpenAIModel>>> GetAvailableModelsAsync(bool forceRefresh = false, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!forceRefresh && _cache.TryGetValue(MODELS_CACHE_KEY, out List<OpenAIModel>? cachedModels) && cachedModels != null)
                {
                    return ServiceResult<List<OpenAIModel>>.SuccessResult(cachedModels
                        .Where(m => m.DeprecationDate == null || m.DeprecationDate > DateTime.UtcNow)
                        .OrderBy(m => m.Name)
                        .ToList());
                }

                // Try to fetch live models from OpenAI API
                var liveModels = await FetchLiveModelsAsync(cancellationToken);
                var allModels = MergeWithStaticModels(liveModels);

                // Cache the results
                _cache.Set(MODELS_CACHE_KEY, allModels, TimeSpan.FromHours(CACHE_EXPIRATION_HOURS));

                var activeModels = allModels
                    .Where(m => m.DeprecationDate == null || m.DeprecationDate > DateTime.UtcNow)
                    .OrderBy(m => m.Name)
                    .ToList();

                return ServiceResult<List<OpenAIModel>>.SuccessResult(activeModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get available models");

                // Fallback to static models
                var fallbackModels = _staticModels
                    .Where(m => m.DeprecationDate == null || m.DeprecationDate > DateTime.UtcNow)
                    .OrderBy(m => m.Name)
                    .ToList();

                return ServiceResult<List<OpenAIModel>>.SuccessResult(fallbackModels);
            }
        }

        /// <summary>
        /// Get models recommended for educational content
        /// </summary>
        public async Task<ServiceResult<List<OpenAIModel>>> GetEducationRecommendedModelsAsync(CancellationToken cancellationToken = default)
        {
            var allModelsResult = await GetAvailableModelsAsync(false, cancellationToken);
            if (!allModelsResult.Success)
                return allModelsResult;

            var educationModels = allModelsResult.Data!
                .Where(m => m.IsRecommendedForEducation)
                .OrderBy(m => m.CostPer1kInputTokens)
                .ToList();

            return ServiceResult<List<OpenAIModel>>.SuccessResult(educationModels);
        }

        /// <summary>
        /// Recommend the best model based on criteria
        /// </summary>
        public async Task<ServiceResult<ModelRecommendation>> RecommendModelAsync(ModelSelectionCriteria criteria, CancellationToken cancellationToken = default)
        {
            try
            {
                var availableModelsResult = await GetAvailableModelsAsync(false, cancellationToken); if (!availableModelsResult.Success)
                    return ServiceResult<ModelRecommendation>.ErrorResult("Failed to get available models");

                var availableModels = availableModelsResult.Data!;
                var recommendation = RecommendModel(criteria, availableModels);

                return ServiceResult<ModelRecommendation>.SuccessResult(recommendation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to recommend model");
                return ServiceResult<ModelRecommendation>.ErrorResult($"Failed to recommend model: {ex.Message}");
            }
        }

        /// <summary>
        /// Get model by ID
        /// </summary>
        public async Task<ServiceResult<OpenAIModel>> GetModelByIdAsync(string modelId, CancellationToken cancellationToken = default)
        {
            try
            {
                var availableModelsResult = await GetAvailableModelsAsync(false, cancellationToken); if (!availableModelsResult.Success)
                    return ServiceResult<OpenAIModel>.ErrorResult("Failed to get available models");

                var model = availableModelsResult.Data!.FirstOrDefault(m => m.Id == modelId);
                if (model == null)
                    return ServiceResult<OpenAIModel>.ErrorResult($"Model '{modelId}' not found");

                return ServiceResult<OpenAIModel>.SuccessResult(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get model by ID: {ModelId}", modelId);
                return ServiceResult<OpenAIModel>.ErrorResult($"Failed to get model: {ex.Message}");
            }
        }

        /// <summary>
        /// Estimate cost for a model and token count
        /// </summary>
        public async Task<ServiceResult<decimal>> EstimateCostAsync(string modelId, int estimatedTokens, CancellationToken cancellationToken = default)
        {
            try
            {
                var modelResult = await GetModelByIdAsync(modelId, cancellationToken); if (!modelResult.Success)
                    return ServiceResult<decimal>.ErrorResult($"Model '{modelId}' not found");

                var cost = EstimateCostInternal(modelResult.Data!, estimatedTokens);
                return ServiceResult<decimal>.SuccessResult(cost);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to estimate cost for model: {ModelId}", modelId);
                return ServiceResult<decimal>.ErrorResult($"Failed to estimate cost: {ex.Message}");
            }
        }

        /// <summary>
        /// Refresh model registry from OpenAI API
        /// </summary>
        public async Task<ServiceResult<bool>> RefreshModelRegistryAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _cache.Remove(MODELS_CACHE_KEY);
                var result = await GetAvailableModelsAsync(true, cancellationToken); return ServiceResult<bool>.SuccessResult(result.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to refresh model registry");
                return ServiceResult<bool>.ErrorResult($"Failed to refresh model registry: {ex.Message}");
            }
        }

        #region Legacy Methods (for backward compatibility)

        /// <summary>
        /// Get all available models (legacy method)        #region Legacy Methods (for backward compatibility)

        /// <summary>
        /// Get all available models (legacy method)
        /// </summary>
        public List<OpenAIModel> GetAvailableModels()
        {
            var result = GetAvailableModelsAsync().Result;
            return result.Success ? result.Data! : _staticModels
                .Where(m => m.DeprecationDate == null || m.DeprecationDate > DateTime.UtcNow)
                .OrderBy(m => m.Name)
                .ToList();
        }

        /// <summary>
        /// Get models recommended for educational content (legacy method)
        /// </summary>
        public List<OpenAIModel> GetEducationRecommendedModels()
        {
            var result = GetEducationRecommendedModelsAsync().Result;
            return result.Success ? result.Data! : _staticModels
                .Where(m => m.IsRecommendedForEducation && (m.DeprecationDate == null || m.DeprecationDate > DateTime.UtcNow))
                .OrderBy(m => m.CostPer1kInputTokens)
                .ToList();
        }

        /// <summary>
        /// Recommend the best model based on criteria (legacy method)
        /// </summary>
        public ModelRecommendation RecommendModel(ModelSelectionCriteria criteria)
        {
            var availableModels = GetAvailableModels();
            return RecommendModel(criteria, availableModels);
        }

        /// <summary>
        /// Get model by ID (legacy method)
        /// </summary>
        public OpenAIModel? GetModelById(string modelId)
        {
            var result = GetModelByIdAsync(modelId).Result;
            return result.Success ? result.Data : null;
        }        /// <summary>
                 /// Estimate cost for a model and token count (legacy method)
                 /// </summary>
        private decimal EstimateCostInternal(OpenAIModel model, int estimatedTokens)
        {
            // Assume 75% input tokens, 25% output tokens (typical for content generation)
            var inputTokens = (int)(estimatedTokens * 0.75);
            var outputTokens = (int)(estimatedTokens * 0.25);

            var inputCost = (inputTokens / 1000m) * model.CostPer1kInputTokens;
            var outputCost = (outputTokens / 1000m) * model.CostPer1kOutputTokens;

            return inputCost + outputCost;
        }

        #endregion        #region Private Methods

        /// <summary>
        /// Fetch live models from OpenAI API (placeholder for future implementation)
        /// </summary>
        private async Task<List<OpenAIModel>> FetchLiveModelsAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Live model fetching not yet implemented, using static models");

                // TODO: Implement actual OpenAI API model fetching when the correct API is available
                // For now, return empty list to fall back to static models
                await Task.CompletedTask; // Satisfy async requirement
                return new List<OpenAIModel>();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch live models from OpenAI API, falling back to static models");
                return new List<OpenAIModel>();
            }
        }        /// <summary>
                 /// Merge live models with static model metadata
                 /// </summary>
        private List<OpenAIModel> MergeWithStaticModels(List<OpenAIModel> liveModels)
        {
            var mergedModels = new List<OpenAIModel>();
            var liveModelIds = liveModels.Select(m => m.Id).ToHashSet();

            // Add all live models
            mergedModels.AddRange(liveModels);

            // Add static models (they contain the most accurate metadata)
            foreach (var staticModel in _staticModels)
            {
                if (!liveModelIds.Contains(staticModel.Id))
                {
                    mergedModels.Add(staticModel);
                }
            }

            return mergedModels;
        }

        /// <summary>
        /// Recommend model based on criteria and available models
        /// </summary>
        private ModelRecommendation RecommendModel(ModelSelectionCriteria criteria, List<OpenAIModel> availableModels)
        {
            // Filter by requirements
            var suitableModels = availableModels.Where(m =>
            {
                // Check token limits
                if (criteria.EstimatedTokens > m.MaxTokens) return false;

                // Check structured output requirement
                if (criteria.RequireStructuredOutput && !m.SupportsStructuredOutput) return false;
                // Check cost constraints
                var estimatedCost = EstimateCostInternal(m, criteria.EstimatedTokens);
                if (estimatedCost > criteria.MaxCostPerRequest) return false;

                return true;
            }).ToList();

            if (!suitableModels.Any())
            {
                // Fallback to default model
                var defaultModel = availableModels.FirstOrDefault(m => m.Id == _config.DefaultModel)
                    ?? availableModels.First();
                return new ModelRecommendation
                {
                    RecommendedModel = defaultModel,
                    Reason = "No models met all criteria. Using default model.",
                    EstimatedCost = EstimateCostInternal(defaultModel, criteria.EstimatedTokens),
                    AlternativeModels = new()
                };
            }

            // Score models based on criteria
            var scoredModels = suitableModels.Select(model => new
            {
                Model = model,
                Score = CalculateModelScore(model, criteria)
            }).OrderByDescending(x => x.Score).ToList();

            var recommended = scoredModels.First();
            var alternatives = scoredModels.Skip(1).Take(3).Select(x => x.Model).ToList(); return new ModelRecommendation
            {
                RecommendedModel = recommended.Model,
                Reason = GetRecommendationReason(recommended.Model, criteria),
                EstimatedCost = EstimateCostInternal(recommended.Model, criteria.EstimatedTokens),
                AlternativeModels = alternatives
            };
        }

        /// <summary>
        /// Estimate cost for a model and token count
        /// </summary>
        public decimal EstimateCost(OpenAIModel model, int estimatedTokens)
        {
            // Assume 75% input tokens, 25% output tokens (typical for content generation)
            var inputTokens = (int)(estimatedTokens * 0.75);
            var outputTokens = (int)(estimatedTokens * 0.25);

            var inputCost = (inputTokens / 1000m) * model.CostPer1kInputTokens;
            var outputCost = (outputTokens / 1000m) * model.CostPer1kOutputTokens;

            return inputCost + outputCost;
        }

        /// <summary>
        /// Initialize the model registry with current OpenAI models
        /// </summary>
        private List<OpenAIModel> InitializeModels()
        {
            return new List<OpenAIModel>
            {
                new OpenAIModel
                {
                    Id = "gpt-4o",
                    Name = "GPT-4o",
                    Description = "Most capable model, excellent for complex educational content",
                    MaxTokens = 128000,
                    CostPer1kInputTokens = 0.005m,
                    CostPer1kOutputTokens = 0.015m,
                    SupportsStructuredOutput = true,
                    IsRecommendedForEducation = true,
                    Strengths = new() { "Complex reasoning", "High accuracy", "Structured output", "Long context" }
                },
                new OpenAIModel
                {
                    Id = "gpt-4o-mini",
                    Name = "GPT-4o Mini",
                    Description = "Fast and cost-effective, great for most educational tasks",
                    MaxTokens = 128000,
                    CostPer1kInputTokens = 0.00015m,
                    CostPer1kOutputTokens = 0.0006m,
                    SupportsStructuredOutput = true,
                    IsRecommendedForEducation = true,
                    Strengths = new() { "Cost-effective", "Fast response", "Good quality", "Long context" }
                },
                new OpenAIModel
                {
                    Id = "gpt-4-turbo",
                    Name = "GPT-4 Turbo",
                    Description = "High performance model with good cost balance",
                    MaxTokens = 128000,
                    CostPer1kInputTokens = 0.01m,
                    CostPer1kOutputTokens = 0.03m,
                    SupportsStructuredOutput = true,
                    IsRecommendedForEducation = true,
                    Strengths = new() { "Reliable", "Well-tested", "Good reasoning", "Structured output" }
                },
                new OpenAIModel
                {
                    Id = "gpt-3.5-turbo",
                    Name = "GPT-3.5 Turbo",
                    Description = "Budget-friendly option for simple worksheets",
                    MaxTokens = 16385,
                    CostPer1kInputTokens = 0.0005m,
                    CostPer1kOutputTokens = 0.0015m,
                    SupportsStructuredOutput = false,
                    IsRecommendedForEducation = false,
                    Strengths = new() { "Very cost-effective", "Fast", "Good for simple tasks" }
                },
                new OpenAIModel
                {
                    Id = "o1-preview",
                    Name = "o1 Preview",
                    Description = "Advanced reasoning model for complex educational analysis",
                    MaxTokens = 128000,
                    CostPer1kInputTokens = 0.015m,
                    CostPer1kOutputTokens = 0.06m,
                    SupportsStructuredOutput = false,
                    IsRecommendedForEducation = false,
                    Strengths = new() { "Advanced reasoning", "Complex problem solving", "Deep analysis" }
                },
                new OpenAIModel
                {
                    Id = "o1-mini",
                    Name = "o1 Mini",
                    Description = "Reasoning model optimized for STEM subjects",
                    MaxTokens = 128000,
                    CostPer1kInputTokens = 0.003m,
                    CostPer1kOutputTokens = 0.012m,
                    SupportsStructuredOutput = false,
                    IsRecommendedForEducation = false,
                    Strengths = new() { "STEM reasoning", "Mathematical accuracy", "Logical analysis" }
                }
            };
        }

        /// <summary>
        /// Calculate score for model based on selection criteria
        /// </summary>
        private double CalculateModelScore(OpenAIModel model, ModelSelectionCriteria criteria)
        {
            double score = 0;

            // Base score for education recommendation
            if (model.IsRecommendedForEducation) score += 10;            // Cost efficiency (lower cost = higher score)
            var estimatedCost = EstimateCostInternal(model, criteria.EstimatedTokens);
            var costScore = Math.Max(0, 10 - (double)(estimatedCost * 10)); // Scale cost to 0-10
            score += costScore;

            // Speed vs Quality preference
            if (criteria.PrioritizeSpeed)
            {
                if (model.Id.Contains("mini") || model.Id.Contains("3.5")) score += 5;
            }

            if (criteria.PrioritizeQuality)
            {
                if (model.Id.Contains("gpt-4o") || model.Id.Contains("o1")) score += 5;
            }

            // Worksheet type specific bonuses
            switch (criteria.WorksheetType)
            {
                case "creative-writing":
                    if (model.Strengths.Contains("Complex reasoning")) score += 3;
                    break;
                case "vocabulary":
                case "grammar":
                    if (model.Strengths.Contains("Fast response")) score += 3;
                    break;
                case "literary-analysis":
                    if (model.Strengths.Contains("Deep analysis")) score += 5;
                    break;
            }

            return score;
        }

        /// <summary>
        /// Generate human-readable recommendation reason
        /// </summary>
        private string GetRecommendationReason(OpenAIModel model, ModelSelectionCriteria criteria)
        {
            var reasons = new List<string>();

            if (model.IsRecommendedForEducation)
                reasons.Add("optimized for educational content");

            var estimatedCost = EstimateCostInternal(model, criteria.EstimatedTokens);
            if (estimatedCost < criteria.MaxCostPerRequest * 0.5m)
                reasons.Add("cost-effective");

            if (criteria.PrioritizeSpeed && model.Strengths.Contains("Fast response"))
                reasons.Add("fast response time");

            if (criteria.PrioritizeQuality && model.Strengths.Contains("High accuracy"))
                reasons.Add("high quality output");

            if (criteria.RequireStructuredOutput && model.SupportsStructuredOutput)
                reasons.Add("supports structured output");

            var reasonText = reasons.Any()
                ? $"Recommended because it is {string.Join(", ", reasons)}."
                : "Best available option for your requirements.";

            return $"{reasonText} Estimated cost: ${estimatedCost:F4}";
        }

        /// <summary>
        /// Initialize the static model registry with current OpenAI models
        /// </summary>
        private List<OpenAIModel> InitializeStaticModels()
        {
            return new List<OpenAIModel>
            {
                new OpenAIModel
                {
                    Id = "gpt-4o",
                    Name = "GPT-4o",
                    Description = "Most capable model, excellent for complex educational content",
                    MaxTokens = 128000,
                    CostPer1kInputTokens = 0.005m,
                    CostPer1kOutputTokens = 0.015m,
                    SupportsStructuredOutput = true,
                    IsRecommendedForEducation = true,
                    Strengths = new() { "Complex reasoning", "High accuracy", "Structured output", "Long context" }
                },
                new OpenAIModel
                {
                    Id = "gpt-4o-mini",
                    Name = "GPT-4o Mini",
                    Description = "Fast and cost-effective, great for most educational tasks",
                    MaxTokens = 128000,
                    CostPer1kInputTokens = 0.00015m,
                    CostPer1kOutputTokens = 0.0006m,
                    SupportsStructuredOutput = true,
                    IsRecommendedForEducation = true,
                    Strengths = new() { "Cost-effective", "Fast response", "Good quality", "Long context" }
                },
                new OpenAIModel
                {
                    Id = "gpt-4-turbo",
                    Name = "GPT-4 Turbo",
                    Description = "High performance model with good cost balance",
                    MaxTokens = 128000,
                    CostPer1kInputTokens = 0.01m,
                    CostPer1kOutputTokens = 0.03m,
                    SupportsStructuredOutput = true,
                    IsRecommendedForEducation = true,
                    Strengths = new() { "Reliable", "Well-tested", "Good reasoning", "Structured output" }
                },
                new OpenAIModel
                {
                    Id = "gpt-3.5-turbo",
                    Name = "GPT-3.5 Turbo",
                    Description = "Budget-friendly option for simple worksheets",
                    MaxTokens = 16385,
                    CostPer1kInputTokens = 0.0005m,
                    CostPer1kOutputTokens = 0.0015m,
                    SupportsStructuredOutput = false,
                    IsRecommendedForEducation = false,
                    Strengths = new() { "Very cost-effective", "Fast", "Good for simple tasks" }
                },
                new OpenAIModel
                {
                    Id = "o1-preview",
                    Name = "o1 Preview",
                    Description = "Advanced reasoning model for complex educational analysis",
                    MaxTokens = 128000,
                    CostPer1kInputTokens = 0.015m,
                    CostPer1kOutputTokens = 0.06m,
                    SupportsStructuredOutput = false,
                    IsRecommendedForEducation = false,
                    Strengths = new() { "Advanced reasoning", "Complex problem solving", "Deep analysis" }
                },
                new OpenAIModel
                {
                    Id = "o1-mini",
                    Name = "o1 Mini",
                    Description = "Reasoning model optimized for STEM subjects",
                    MaxTokens = 128000,
                    CostPer1kInputTokens = 0.003m,
                    CostPer1kOutputTokens = 0.012m,
                    SupportsStructuredOutput = false,
                    IsRecommendedForEducation = false,
                    Strengths = new() { "STEM reasoning", "Mathematical accuracy", "Logical analysis" }
                }
            };
        }
    }
}
