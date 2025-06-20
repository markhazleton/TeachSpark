namespace TeachSpark.Web.Configuration
{
    /// <summary>
    /// Configuration for LLM service providers
    /// </summary>
    public class LlmConfiguration
    {
        public const string SectionName = "LlmConfiguration";

        public string Provider { get; set; } = "OpenAI";
        public string ApiKey { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
        public string DefaultModel { get; set; } = "gpt-4o-mini";
        public int MaxTokens { get; set; } = 4000;
        public double Temperature { get; set; } = 0.7;
        public int TimeoutSeconds { get; set; } = 60;
        public int MaxRetries { get; set; } = 3;

        // Cost management
        public decimal MaxCostPerRequest { get; set; } = 1.00m;
        public decimal DailyCostLimit { get; set; } = 50.00m;

        // Rate limiting
        public int RequestsPerMinute { get; set; } = 20;
        public int RequestsPerDay { get; set; } = 1000;

        // Caching
        public bool EnableCaching { get; set; } = true;
        public int CacheExpirationMinutes { get; set; } = 60;
    }

    /// <summary>
    /// Configuration for worksheet generation features
    /// </summary>
    public class WorksheetGenerationConfiguration
    {
        public const string SectionName = "WorksheetGeneration";

        public int DefaultQuestionCount { get; set; } = 10;
        public int MaxQuestionCount { get; set; } = 25;

        public List<string> SupportedWorksheetTypes { get; set; } = new()
        {
            "reading-comprehension",
            "vocabulary",
            "grammar",
            "creative-writing",
            "literary-analysis"
        };

        public List<string> SupportedDifficultyLevels { get; set; } = new()
        {
            "simplified",
            "standard",
            "advanced"
        };

        public bool EnableContentValidation { get; set; } = true;
        public bool RequireStandardsAlignment { get; set; } = true;
    }

    /// <summary>
    /// Runtime metrics for LLM usage tracking
    /// </summary>
    public class LlmUsageMetrics
    {
        public int TotalRequests { get; set; }
        public int SuccessfulRequests { get; set; }
        public int FailedRequests { get; set; }
        public decimal TotalCost { get; set; }
        public long TotalTokensUsed { get; set; }
        public TimeSpan TotalProcessingTime { get; set; }
        public DateTime LastReset { get; set; } = DateTime.UtcNow;

        public double SuccessRate => TotalRequests > 0 ? (double)SuccessfulRequests / TotalRequests * 100 : 0;
        public decimal AverageCostPerRequest => TotalRequests > 0 ? TotalCost / TotalRequests : 0;
        public double AverageTokensPerRequest => TotalRequests > 0 ? (double)TotalTokensUsed / TotalRequests : 0;
    }
}
