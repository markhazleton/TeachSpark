using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TeachSpark.Web.Services.Models
{
    /// <summary>
    /// Request model for generating worksheet content
    /// </summary>
    public class WorksheetGenerationRequest
    {
        [Required]
        public string SourceText { get; set; } = string.Empty;

        [Required]
        public string WorksheetType { get; set; } = string.Empty; // "reading-comprehension", "vocabulary", etc.

        public int? CommonCoreStandardId { get; set; }
        public int? BloomLevelId { get; set; }
        public int? TemplateId { get; set; }

        [Required]
        public string DifficultyLevel { get; set; } = "standard"; // "simplified", "standard", "advanced"

        public List<string> AccessibilityOptions { get; set; } = new();
        public List<string> Tags { get; set; } = new();        // Generation parameters
        public string? CustomInstructions { get; set; }
        public int MaxQuestions { get; set; } = 10;
        public bool IncludeAnswerKey { get; set; } = true;
        public string PreferredLlmModel { get; set; } = string.Empty;

        // Dropdown lists for the form
        public List<SelectListItem> WorksheetTypeOptions { get; set; } = new();
        public List<SelectListItem> DifficultyLevelOptions { get; set; } = new();
        public List<SelectListItem> CommonCoreStandardOptions { get; set; } = new();
        public List<SelectListItem> BloomLevelOptions { get; set; } = new();
        public List<SelectListItem> TemplateOptions { get; set; } = new();
        public List<SelectListItem> AvailableModelOptions { get; set; } = new();
    }/// <summary>
     /// Result of worksheet content generation
     /// </summary>
    public class WorksheetContentResult
    {
        public string MarkdownContent { get; set; } = string.Empty;
        public string RenderedHtml { get; set; } = string.Empty;
        public string GeneratedTitle { get; set; } = string.Empty;
        public string GeneratedDescription { get; set; } = string.Empty;

        // Generation metadata
        public string LlmModel { get; set; } = string.Empty;
        public string GenerationPrompt { get; set; } = string.Empty;
        public decimal GenerationCost { get; set; }
        public TimeSpan GenerationTime { get; set; }
        public int TokensUsed { get; set; }

        // Quality metrics
        public double ConfidenceScore { get; set; }
        public List<string> Warnings { get; set; } = new();

        // Markdown parsing results
        public List<string> ExtractedQuestions { get; set; } = new();
        public bool HasAnswerKey { get; set; }
        public int EstimatedDurationMinutes { get; set; }
    }

    /// <summary>
    /// Validation result for generated content
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public double QualityScore { get; set; }
        public Dictionary<string, object> Metrics { get; set; } = new();
    }

    /// <summary>
    /// Content analysis metrics
    /// </summary>
    public class ContentMetrics
    {
        public int WordCount { get; set; }
        public int QuestionCount { get; set; }
        public double ReadabilityScore { get; set; }
        public string EstimatedGradeLevel { get; set; } = string.Empty;
        public List<string> KeyTopics { get; set; } = new();
        public Dictionary<string, int> BloomLevelDistribution { get; set; } = new();
    }

    /// <summary>
    /// Worksheet variant for generating multiple versions
    /// </summary>
    public class WorksheetVariant
    {
        public string DifficultyLevel { get; set; } = string.Empty;
        public WorksheetContentResult Content { get; set; } = new();
        public string VariantDescription { get; set; } = string.Empty;
    }    /// <summary>
         /// Prompt template for different worksheet types
         /// </summary>
    public class PromptTemplate
    {
        public string WorksheetType { get; set; } = string.Empty;
        public string Template { get; set; } = string.Empty;
        public Dictionary<string, string> Variables { get; set; } = new();
        public List<string> RequiredFields { get; set; } = new();
    }

    /// <summary>
    /// Available OpenAI models with their capabilities and costs
    /// </summary>
    public class OpenAIModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int MaxTokens { get; set; }
        public decimal CostPer1kInputTokens { get; set; }
        public decimal CostPer1kOutputTokens { get; set; }
        public bool SupportsStructuredOutput { get; set; }
        public bool IsRecommendedForEducation { get; set; }
        public DateTime? DeprecationDate { get; set; }
        public List<string> Strengths { get; set; } = new();
    }

    /// <summary>
    /// Model selection criteria for different use cases
    /// </summary>
    public class ModelSelectionCriteria
    {
        public string WorksheetType { get; set; } = string.Empty;
        public string DifficultyLevel { get; set; } = string.Empty;
        public int EstimatedTokens { get; set; }
        public decimal MaxCostPerRequest { get; set; }
        public bool RequireStructuredOutput { get; set; }
        public bool PrioritizeSpeed { get; set; }
        public bool PrioritizeQuality { get; set; }
    }    /// <summary>
         /// Model recommendation result
         /// </summary>
    public class ModelRecommendation
    {
        public OpenAIModel RecommendedModel { get; set; } = new();
        public string Reason { get; set; } = string.Empty;
        public decimal EstimatedCost { get; set; }
        public List<OpenAIModel> AlternativeModels { get; set; } = new();
    }

    /// <summary>
    /// Metadata about an LLM call for logging purposes
    /// </summary>
    public class LlmCallMetadata
    {
        public string UserId { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string ModelUsed { get; set; } = string.Empty;
        public TimeSpan Duration { get; set; }
        public int TokensUsed { get; set; }
        public decimal Cost { get; set; }
        public float Temperature { get; set; }
        public int MaxTokens { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string RequestId { get; set; } = Guid.NewGuid().ToString();
    }
}
