using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using System.Diagnostics;
using TeachSpark.Web.Configuration;
using TeachSpark.Web.Services.Interfaces;
using TeachSpark.Web.Services.Models;
using TeachSpark.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace TeachSpark.Web.Services.Implementations
{    /// <summary>
     /// OpenAI implementation of the LLM service using Microsoft.Extensions.AI
     /// </summary>
    public class OpenAILlmService : ILlmService
    {
        private readonly ChatClient _chatClient;
        private readonly IModelRegistryService _modelRegistry;
        private readonly IMemoryCache _cache;
        private readonly LlmConfiguration _config;
        private readonly LlmUsageMetrics _metrics;
        private readonly MarkdownRenderingService _markdownService;
        private readonly ILlmLoggingService _llmLoggingService;
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<OpenAILlmService> _logger;

        public OpenAILlmService(
            ChatClient chatClient,
            IModelRegistryService modelRegistry,
            IMemoryCache cache,
            IOptions<LlmConfiguration> config,
            LlmUsageMetrics metrics,
            MarkdownRenderingService markdownService,
            ILlmLoggingService llmLoggingService,
            ApplicationDbContext dbContext,
            ILogger<OpenAILlmService> logger)
        {
            _chatClient = chatClient;
            _modelRegistry = modelRegistry;
            _cache = cache;
            _config = config.Value;
            _metrics = metrics;
            _markdownService = markdownService;
            _llmLoggingService = llmLoggingService;
            _dbContext = dbContext;
            _logger = logger;
        }/// <summary>
         /// Generate worksheet content using OpenAI
         /// </summary>
        public async Task<ServiceResult<WorksheetContentResult>> GenerateWorksheetContentAsync(
            WorksheetGenerationRequest request,
            string? userId = null,
            string? userEmail = null,
            CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();
            var requestId = Guid.NewGuid().ToString();

            try
            {
                _logger.LogInformation("Starting worksheet generation for type: {WorksheetType}, RequestId: {RequestId}",
                    request.WorksheetType, requestId);

                // Validate input
                if (string.IsNullOrWhiteSpace(request.SourceText))
                {
                    return ServiceResult<WorksheetContentResult>.ErrorResult("Source text is required");
                }                // Generate prompts
                var systemPrompt = GetSystemPrompt();
                var userPrompt = await GeneratePromptAsync(request);

                _logger.LogDebug("Generated prompts for RequestId: {RequestId}", requestId);

                // Create chat messages
                var messages = new List<ChatMessage>
                {
                    ChatMessage.CreateSystemMessage(systemPrompt),
                    ChatMessage.CreateUserMessage(userPrompt)
                };

                // Configure chat options
                var chatOptions = new ChatCompletionOptions
                {
                    Temperature = (float)_config.Temperature,
                    MaxOutputTokenCount = _config.MaxTokens
                };                // Prepare metadata for logging
                var metadata = new LlmCallMetadata
                {
                    RequestId = requestId,
                    UserId = userId ?? "Unknown",
                    UserEmail = userEmail ?? "Unknown",
                    ModelUsed = request.PreferredLlmModel ?? _config.DefaultModel,
                    Temperature = (float)_config.Temperature,
                    MaxTokens = _config.MaxTokens,
                    Timestamp = DateTime.UtcNow
                };

                // Call OpenAI API
                var response = await _chatClient.CompleteChatAsync(messages, chatOptions, cancellationToken);

                if (string.IsNullOrEmpty(response.Value.Content[0].Text))
                {
                    var error = new InvalidOperationException("Received empty response from AI service");

                    // Log the error to the special LLM log file
                    await _llmLoggingService.LogLlmErrorAsync(request, systemPrompt, userPrompt, error, metadata);

                    return ServiceResult<WorksheetContentResult>.ErrorResult("Received empty response from AI service");
                }

                var markdownContent = response.Value.Content[0].Text;

                // Update metadata with response information
                stopwatch.Stop();
                metadata.Duration = stopwatch.Elapsed;
                metadata.TokensUsed = (response.Value.Usage?.OutputTokenCount ?? 0) + (response.Value.Usage?.InputTokenCount ?? 0);

                // Calculate cost
                try
                {
                    var modelResult = await _modelRegistry.GetEducationRecommendedModelsAsync();
                    if (modelResult.Success && modelResult.Data != null)
                    {
                        var usedModel = modelResult.Data.FirstOrDefault(m => m.Id == metadata.ModelUsed);
                        if (usedModel != null)
                        {
                            var inputTokens = response.Value.Usage?.InputTokenCount ?? 0;
                            var outputTokens = response.Value.Usage?.OutputTokenCount ?? 0;
                            metadata.Cost = (decimal)inputTokens / 1000 * usedModel.CostPer1kInputTokens +
                                          (decimal)outputTokens / 1000 * usedModel.CostPer1kOutputTokens;
                        }
                    }
                }
                catch (Exception costEx)
                {
                    _logger.LogWarning(costEx, "Failed to calculate cost for RequestId: {RequestId}", requestId);
                }

                // Log successful LLM interaction to special file
                await _llmLoggingService.LogLlmInteractionAsync(request, systemPrompt, userPrompt, markdownContent, metadata);

                // Process and validate the response
                var result = await ProcessAIResponse(markdownContent, request, response.Value.Usage, stopwatch.Elapsed);

                UpdateMetrics(true, result.GenerationCost, result.TokensUsed, stopwatch.Elapsed);

                _logger.LogInformation("Successfully generated worksheet content for RequestId: {RequestId}", requestId);
                return ServiceResult<WorksheetContentResult>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                UpdateMetrics(false, 0, 0, stopwatch.Elapsed);                // Create metadata for error logging
                var errorMetadata = new LlmCallMetadata
                {
                    RequestId = requestId,
                    UserId = userId ?? "Unknown",
                    UserEmail = userEmail ?? "Unknown",
                    ModelUsed = request.PreferredLlmModel ?? _config.DefaultModel,
                    Temperature = (float)_config.Temperature,
                    MaxTokens = _config.MaxTokens,
                    Duration = stopwatch.Elapsed,
                    Timestamp = DateTime.UtcNow
                };

                // Log the error to the special LLM log file
                try
                {
                    var systemPrompt = GetSystemPrompt();
                    var userPrompt = await GeneratePromptAsync(request);
                    await _llmLoggingService.LogLlmErrorAsync(request, systemPrompt, userPrompt, ex, errorMetadata);
                }
                catch (Exception loggingEx)
                {
                    _logger.LogError(loggingEx, "Failed to log LLM error for RequestId: {RequestId}", requestId);
                }

                _logger.LogError(ex, "Failed to generate worksheet content for RequestId: {RequestId}", requestId);
                return ServiceResult<WorksheetContentResult>.ErrorResult(
                    $"Failed to generate worksheet: {ex.Message}");
            }
        }

        /// <summary>
        /// Get available models for user selection
        /// </summary>
        public async Task<ServiceResult<List<string>>> GetAvailableModelsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var modelsResult = await _modelRegistry.GetEducationRecommendedModelsAsync(cancellationToken);
                if (!modelsResult.Success || modelsResult.Data == null)
                {
                    return ServiceResult<List<string>>.ErrorResult("Failed to retrieve available models");
                }

                var models = modelsResult.Data.Select(m => m.Id).ToList();
                return ServiceResult<List<string>>.SuccessResult(models);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get available models");
                return ServiceResult<List<string>>.ErrorResult("Failed to retrieve available models");
            }
        }

        /// <summary>
        /// Validate OpenAI configuration
        /// </summary>
        public async Task<ServiceResult<bool>> ValidateConfigurationAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                // Test the connection with a simple request
                var testMessages = new List<ChatMessage>
                {
                    ChatMessage.CreateUserMessage("Test connection. Please respond with 'OK'.")
                };

                var testOptions = new ChatCompletionOptions
                {
                    MaxOutputTokenCount = 10,
                    Temperature = 0
                };

                var response = await _chatClient.CompleteChatAsync(testMessages, testOptions, cancellationToken);

                bool isValid = !string.IsNullOrEmpty(response.Value.Content[0].Text);
                return ServiceResult<bool>.SuccessResult(isValid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OpenAI configuration validation failed");
                return ServiceResult<bool>.ErrorResult($"Configuration validation failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Update LLM usage metrics
        /// </summary>
        private void UpdateMetrics(bool success, decimal cost, int tokens, TimeSpan duration)
        {
            _metrics.TotalRequests++;
            _metrics.TotalCost += cost;
            _metrics.TotalTokensUsed += tokens;
            _metrics.TotalProcessingTime = _metrics.TotalProcessingTime.Add(duration);

            if (success)
                _metrics.SuccessfulRequests++;
            else
                _metrics.FailedRequests++;
        }        /// <summary>
                 /// Generate the appropriate prompt based on worksheet type and request parameters
                 /// </summary>
        private async Task<string> GeneratePromptAsync(WorksheetGenerationRequest request)
        {
            var promptBuilder = new System.Text.StringBuilder();

            // Base prompt based on worksheet type
            var basePrompt = request.WorksheetType switch
            {
                "reading-comprehension" => GenerateReadingComprehensionPrompt(request),
                "vocabulary" => GenerateVocabularyPrompt(request),
                "grammar" => GenerateGrammarPrompt(request),
                "creative-writing" => GenerateCreativeWritingPrompt(request),
                "literary-analysis" => GenerateLiteraryAnalysisPrompt(request),
                _ => GenerateDefaultPrompt(request)
            };

            promptBuilder.AppendLine(basePrompt);

            // Add Common Core Standard information if specified
            if (request.CommonCoreStandardId.HasValue)
            {
                try
                {
                    var standard = await _dbContext.CommonCoreStandards
                        .AsNoTracking()
                        .FirstOrDefaultAsync(s => s.Id == request.CommonCoreStandardId.Value);

                    if (standard != null)
                    {
                        promptBuilder.AppendLine($"\n**Common Core Standard Alignment:**");
                        promptBuilder.AppendLine($"- Code: {standard.Code}");
                        promptBuilder.AppendLine($"- Grade: {standard.Grade}");
                        promptBuilder.AppendLine($"- Subject: {standard.Subject}");
                        promptBuilder.AppendLine($"- Domain: {standard.Domain}");
                        if (!string.IsNullOrEmpty(standard.Category))
                        {
                            promptBuilder.AppendLine($"- Category: {standard.Category}");
                        }
                        promptBuilder.AppendLine($"- Description: {standard.Description}");
                        if (!string.IsNullOrEmpty(standard.ExampleActivities))
                        {
                            promptBuilder.AppendLine($"- Example Activities: {standard.ExampleActivities}");
                        }
                        promptBuilder.AppendLine("Ensure all questions and activities align with this specific standard's requirements and learning objectives.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to fetch Common Core Standard with ID: {StandardId}", request.CommonCoreStandardId);
                }
            }

            // Add Bloom's Taxonomy level information if specified
            if (request.BloomLevelId.HasValue)
            {
                try
                {
                    var bloomLevel = await _dbContext.BloomLevels
                        .AsNoTracking()
                        .FirstOrDefaultAsync(b => b.Id == request.BloomLevelId.Value);

                    if (bloomLevel != null)
                    {
                        promptBuilder.AppendLine($"\n**Bloom's Taxonomy Level:**");
                        promptBuilder.AppendLine($"- Level: {bloomLevel.Name} (Level {bloomLevel.Order}/6)");
                        promptBuilder.AppendLine($"- Description: {bloomLevel.Description}");
                        promptBuilder.AppendLine($"- Action Verbs: {bloomLevel.ActionVerbs}");
                        if (!string.IsNullOrEmpty(bloomLevel.Examples))
                        {
                            promptBuilder.AppendLine($"- Examples: {bloomLevel.Examples}");
                        }
                        promptBuilder.AppendLine($"Focus your questions on the '{bloomLevel.Name}' cognitive level, using appropriate action verbs and cognitive demands for this level.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to fetch Bloom Level with ID: {BloomLevelId}", request.BloomLevelId);
                }
            }

            // Add source text
            promptBuilder.AppendLine("\n**Source Text:**");
            promptBuilder.AppendLine(request.SourceText);

            // Add specific requirements
            promptBuilder.AppendLine($"\n**Requirements:**");
            promptBuilder.AppendLine($"- Generate exactly {request.MaxQuestions} questions");
            promptBuilder.AppendLine($"- Difficulty level: {request.DifficultyLevel}");

            if (request.IncludeAnswerKey)
            {
                promptBuilder.AppendLine("- Include an answer key at the end");
            }

            if (!string.IsNullOrWhiteSpace(request.CustomInstructions))
            {
                promptBuilder.AppendLine($"- Additional instructions: {request.CustomInstructions}");
            }

            promptBuilder.AppendLine("\n**Output Format:**");
            promptBuilder.AppendLine("Return your response as properly formatted Markdown with clear headings, questions, and if requested, an answer key section.");

            return promptBuilder.ToString();
        }

        /// <summary>
        /// Generate reading comprehension specific prompt
        /// </summary>
        private string GenerateReadingComprehensionPrompt(WorksheetGenerationRequest request)
        {
            return @"Create a reading comprehension worksheet based on the provided text. 

Generate thoughtful questions that test various levels of understanding:
- Literal comprehension (facts, details)
- Inferential thinking (reading between the lines)
- Critical analysis (evaluation, opinion)
- Vocabulary in context

Make sure questions are appropriate for 8th grade level and encourage deep thinking about the text.";
        }

        /// <summary>
        /// Generate vocabulary specific prompt
        /// </summary>
        private string GenerateVocabularyPrompt(WorksheetGenerationRequest request)
        {
            return @"Create a vocabulary worksheet using words from the provided text.

Select challenging but age-appropriate vocabulary words and create exercises such as:
- Definition matching
- Context clues
- Synonym/antonym identification
- Using words in original sentences

Focus on words that are important for 8th grade reading comprehension and academic success.";
        }

        /// <summary>
        /// Generate grammar specific prompt
        /// </summary>
        private string GenerateGrammarPrompt(WorksheetGenerationRequest request)
        {
            return @"Create a grammar worksheet using the provided text as source material.

Focus on grade-appropriate grammar concepts such as:
- Parts of speech identification
- Sentence structure analysis
- Punctuation and capitalization
- Subject-verb agreement
- Proper usage of modifiers

Use examples from the text to make the exercises meaningful and contextual.";
        }

        /// <summary>
        /// Generate creative writing specific prompt
        /// </summary>
        private string GenerateCreativeWritingPrompt(WorksheetGenerationRequest request)
        {
            return @"Create creative writing prompts inspired by the provided text.

Generate engaging prompts that encourage students to:
- Explore themes and ideas from the text
- Practice different writing styles and formats
- Develop characters and settings
- Express personal connections and responses

Make prompts open-ended enough to allow for creative expression while providing clear direction.";
        }

        /// <summary>
        /// Generate literary analysis specific prompt
        /// </summary>
        private string GenerateLiteraryAnalysisPrompt(WorksheetGenerationRequest request)
        {
            return @"Create a literary analysis worksheet for the provided text.

Focus on analytical thinking with questions about:
- Literary devices and techniques
- Character development and motivation
- Theme identification and analysis
- Author's purpose and perspective
- Text structure and organization

Encourage students to support their answers with evidence from the text.";
        }

        /// <summary>
        /// Generate default prompt for unspecified worksheet types
        /// </summary>
        private string GenerateDefaultPrompt(WorksheetGenerationRequest request)
        {
            return @"Create an educational worksheet based on the provided text.

Generate thoughtful, age-appropriate questions that encourage:
- Reading comprehension
- Critical thinking
- Text analysis
- Personal reflection

Make the worksheet engaging and educational for 8th grade students.";
        }

        /// <summary>
        /// Get the system prompt that defines the AI's role and behavior
        /// </summary>
        private string GetSystemPrompt()
        {
            return @"You are an expert educational content creator specializing in creating high-quality worksheets for middle school students (grades 6-8). 

Your role is to:
- Create engaging, standards-aligned educational content
- Ensure age-appropriate difficulty and language
- Generate clear, well-structured questions
- Provide accurate and helpful answer keys when requested
- Use proper educational pedagogy and best practices

Always format your output as clean, well-organized Markdown with:
- Clear headings using # and ##
- Numbered questions
- Proper formatting for readability
- Separate sections for different types of content

Maintain a professional, educational tone while keeping content engaging for students.";
        }

        /// <summary>
        /// Process the AI response and create the final worksheet content result
        /// </summary>
        private async Task<WorksheetContentResult> ProcessAIResponse(
            string markdownContent,
            WorksheetGenerationRequest request,
            OpenAI.Chat.ChatTokenUsage? usage,
            TimeSpan generationTime)
        {
            // Calculate costs based on token usage
            var tokensUsed = usage?.OutputTokenCount ?? 0;
            var inputTokens = usage?.InputTokenCount ?? 0;
            var totalTokens = tokensUsed + inputTokens;

            // Estimate cost (simplified calculation)
            decimal estimatedCost = 0.01m; // Default fallback
            try
            {
                var modelResult = await _modelRegistry.GetEducationRecommendedModelsAsync();
                if (modelResult.Success && modelResult.Data != null)
                {
                    var usedModel = modelResult.Data.FirstOrDefault(m => m.Id == (request.PreferredLlmModel ?? _config.DefaultModel));
                    if (usedModel != null)
                    {
                        estimatedCost = (decimal)inputTokens / 1000 * usedModel.CostPer1kInputTokens +
                                       (decimal)tokensUsed / 1000 * usedModel.CostPer1kOutputTokens;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to calculate accurate cost, using estimate");
            }

            // Render markdown to HTML
            var renderedHtml = RenderMarkdownToHtml(markdownContent);

            // Extract metadata from the generated content
            var extractedQuestions = ExtractQuestions(markdownContent);
            var hasAnswerKey = markdownContent.ToLower().Contains("answer") && markdownContent.ToLower().Contains("key");
            var estimatedDuration = CalculateEstimatedDuration(extractedQuestions.Count, request.WorksheetType);

            // Generate title and description if not explicitly provided
            var generatedTitle = ExtractTitle(markdownContent) ?? $"{FormatWorksheetTypeName(request.WorksheetType)} Worksheet";
            var generatedDescription = $"AI-generated {request.WorksheetType} worksheet with {extractedQuestions.Count} questions";

            // Analyze content quality
            var confidenceScore = CalculateConfidenceScore(markdownContent, request);
            var warnings = GenerateWarnings(markdownContent, request);

            return new WorksheetContentResult
            {
                MarkdownContent = markdownContent,
                RenderedHtml = renderedHtml,
                GeneratedTitle = generatedTitle,
                GeneratedDescription = generatedDescription,
                LlmModel = request.PreferredLlmModel ?? _config.DefaultModel,
                GenerationPrompt = "AI-generated educational content", // Simplified for security
                GenerationCost = estimatedCost,
                GenerationTime = generationTime,
                TokensUsed = totalTokens,
                ConfidenceScore = confidenceScore,
                Warnings = warnings,
                ExtractedQuestions = extractedQuestions,
                HasAnswerKey = hasAnswerKey,
                EstimatedDurationMinutes = estimatedDuration
            };
        }

        /// <summary>
        /// Render markdown content to HTML
        /// </summary>
        private string RenderMarkdownToHtml(string markdownContent)
        {
            try
            {
                return _markdownService.RenderToHtml(markdownContent);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to render markdown to HTML, returning original content");
                return $"<pre>{System.Web.HttpUtility.HtmlEncode(markdownContent)}</pre>";
            }
        }

        /// <summary>
        /// Extract questions from markdown content
        /// </summary>
        private List<string> ExtractQuestions(string content)
        {
            if (string.IsNullOrEmpty(content))
                return new List<string>();

            var questions = new List<string>();
            var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);

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
        /// Extract title from markdown content
        /// </summary>
        private string? ExtractTitle(string content)
        {
            var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            var firstLine = lines.FirstOrDefault()?.Trim();
            return firstLine?.StartsWith("#") == true ? firstLine.TrimStart('#').Trim() : null;
        }

        /// <summary>
        /// Calculate estimated duration for completing the worksheet
        /// </summary>
        private int CalculateEstimatedDuration(int questionCount, string worksheetType)
        {
            // Base time per question by type (in minutes)
            var baseTimePerQuestion = worksheetType switch
            {
                "reading-comprehension" => 3,
                "vocabulary" => 2,
                "grammar" => 2,
                "creative-writing" => 5,
                "literary-analysis" => 4,
                _ => 3
            };

            return Math.Max(10, questionCount * baseTimePerQuestion);
        }

        /// <summary>
        /// Calculate confidence score for the generated content
        /// </summary>
        private double CalculateConfidenceScore(string content, WorksheetGenerationRequest request)
        {
            double score = 0.5; // Base score

            // Check content length
            if (content.Length > 500) score += 0.1;
            if (content.Length > 1000) score += 0.1;

            // Check for proper formatting
            if (content.Contains("#")) score += 0.1; // Has headings
            if (content.Contains("?")) score += 0.1; // Has questions

            // Check for answer key if requested
            if (request.IncludeAnswerKey && content.ToLower().Contains("answer"))
                score += 0.1;

            return Math.Min(1.0, score);
        }

        /// <summary>
        /// Generate warnings for the content
        /// </summary>
        private List<string> GenerateWarnings(string content, WorksheetGenerationRequest request)
        {
            var warnings = new List<string>();

            if (content.Length < 200)
                warnings.Add("Generated content is shorter than expected");

            if (request.IncludeAnswerKey && !content.ToLower().Contains("answer"))
                warnings.Add("Answer key was requested but may not be present in the generated content");

            var questionCount = ExtractQuestions(content).Count;
            if (questionCount < request.MaxQuestions)
                warnings.Add($"Generated {questionCount} questions instead of requested {request.MaxQuestions}");

            return warnings;
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
    }
}
