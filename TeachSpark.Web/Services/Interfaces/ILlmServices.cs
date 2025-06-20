using TeachSpark.Web.Services.Models;

namespace TeachSpark.Web.Services.Interfaces
{
    /// <summary>
    /// Service for managing OpenAI model registry and dynamic model selection
    /// </summary>
    public interface IModelRegistryService
    {
        Task<ServiceResult<List<OpenAIModel>>> GetAvailableModelsAsync(bool forceRefresh = false, CancellationToken cancellationToken = default);

        Task<ServiceResult<List<OpenAIModel>>> GetEducationRecommendedModelsAsync(CancellationToken cancellationToken = default);

        Task<ServiceResult<ModelRecommendation>> RecommendModelAsync(ModelSelectionCriteria criteria, CancellationToken cancellationToken = default);

        Task<ServiceResult<OpenAIModel>> GetModelByIdAsync(string modelId, CancellationToken cancellationToken = default);

        Task<ServiceResult<decimal>> EstimateCostAsync(string modelId, int estimatedTokens, CancellationToken cancellationToken = default);

        Task<ServiceResult<bool>> RefreshModelRegistryAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Core LLM service for content generation
    /// </summary>
    public interface ILlmService
    {
        Task<ServiceResult<WorksheetContentResult>> GenerateWorksheetContentAsync(
            WorksheetGenerationRequest request,
            CancellationToken cancellationToken = default);

        Task<ServiceResult<List<string>>> GetAvailableModelsAsync(CancellationToken cancellationToken = default);

        Task<ServiceResult<bool>> ValidateConfigurationAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Service for generating prompts based on educational standards and requirements
    /// </summary>
    public interface IPromptEngineService
    {
        Task<ServiceResult<string>> GenerateWorksheetPromptAsync(WorksheetGenerationRequest request);

        Task<ServiceResult<string>> GenerateBloomLevelPromptAsync(int bloomLevelId, string baseContent);

        Task<ServiceResult<string>> GenerateStandardsAlignmentPromptAsync(int standardId, string content);

        Task<ServiceResult<string>> GenerateDifferentiationPromptAsync(string difficultyLevel, string baseContent);
    }

    /// <summary>
    /// Service for validating and processing LLM-generated content
    /// </summary>
    public interface IContentValidationService
    {
        Task<ServiceResult<ValidationResult>> ValidateWorksheetContentAsync(string content, WorksheetGenerationRequest originalRequest);

        Task<ServiceResult<ValidationResult>> ValidateStandardsAlignmentAsync(string content, int standardId);

        Task<ServiceResult<string>> SanitizeContentAsync(string content);

        Task<ServiceResult<ContentMetrics>> AnalyzeContentMetricsAsync(string content);
    }

    /// <summary>
    /// Enhanced worksheet service with LLM generation capabilities
    /// </summary>
    public interface IWorksheetGenerationService : IWorksheetService
    {
        Task<ServiceResult<WorksheetViewModel>> GenerateWorksheetAsync(
            WorksheetGenerationRequest request,
            string userId,
            CancellationToken cancellationToken = default);

        Task<ServiceResult<WorksheetViewModel>> RegenerateWithDifferentComplexityAsync(
            int worksheetId,
            string newDifficultyLevel,
            string userId,
            CancellationToken cancellationToken = default);

        Task<ServiceResult<List<WorksheetVariant>>> GenerateVariantsAsync(
            int templateId,
            string sourceText,
            List<string> difficultyLevels,
            string userId,
            CancellationToken cancellationToken = default);
    }
}
