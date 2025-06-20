using Microsoft.Extensions.AI;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using TeachSpark.Web.Configuration;
using TeachSpark.Web.Services.Interfaces;
using TeachSpark.Web.Services.Models;

namespace TeachSpark.Web.Services.Implementations
{
    /// <summary>
    /// OpenAI implementation of the LLM service using Microsoft.Extensions.AI
    /// </summary>
    public class OpenAILlmService : ILlmService
    {
        private readonly IChatClient _chatClient;
        private readonly IModelRegistryService _modelRegistry;
        private readonly IMemoryCache _cache;
        private readonly LlmConfiguration _config;
        private readonly LlmUsageMetrics _metrics;
        private readonly MarkdownRenderingService _markdownService;
        private readonly ILogger<OpenAILlmService> _logger;

        public OpenAILlmService(
            IChatClient chatClient,
            IModelRegistryService modelRegistry,
            IMemoryCache cache,
            IOptions<LlmConfiguration> config,
            LlmUsageMetrics metrics,
            MarkdownRenderingService markdownService,
            ILogger<OpenAILlmService> logger)
        {
            _chatClient = chatClient;
            _modelRegistry = modelRegistry;
            _cache = cache;
            _config = config.Value;
            _metrics = metrics;
            _markdownService = markdownService;
            _logger = logger;
        }/// <summary>
         /// Generate worksheet content using OpenAI
         /// </summary>
        public async Task<ServiceResult<WorksheetContentResult>> GenerateWorksheetContentAsync(
            WorksheetGenerationRequest request,
            CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                _logger.LogInformation("Starting worksheet generation for type: {WorksheetType}", request.WorksheetType);

                // Validate input
                if (string.IsNullOrWhiteSpace(request.SourceText))
                {
                    return ServiceResult<WorksheetContentResult>.ErrorResult("Source text is required");
                }

                // Generate prompt based on worksheet type and requirements
                var prompt = GeneratePrompt(request);
                _logger.LogDebug("Generated prompt: {Prompt}", prompt);

                // Create chat messages
                var messages = new List<Microsoft.Extensions.AI.ChatMessage>
                {
                    new(Microsoft.Extensions.AI.ChatRole.System, GetSystemPrompt()),
                    new(Microsoft.Extensions.AI.ChatRole.User, prompt)
                };

                // Configure chat options
                var chatOptions = new Microsoft.Extensions.AI.ChatOptions
                {
                    Temperature = (float)_config.Temperature,
                    MaxOutputTokens = _config.MaxTokens,
                    ModelId = request.PreferredLlmModel ?? _config.DefaultModel
                };

                // Call OpenAI API
                var response = await _chatClient.CompleteAsync(messages, chatOptions, cancellationToken);

                if (response?.Message?.Text == null)
                {
                    return ServiceResult<WorksheetContentResult>.ErrorResult("Received empty response from AI service");
                }

                var markdownContent = response.Message.Text;

                // Process and validate the response
                var result = await ProcessAIResponse(markdownContent, request, response.Usage, stopwatch.Elapsed);

                stopwatch.Stop();
                UpdateMetrics(true, result.GenerationCost, result.TokensUsed, stopwatch.Elapsed);

                return ServiceResult<WorksheetContentResult>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                UpdateMetrics(false, 0, 0, stopwatch.Elapsed);

                _logger.LogError(ex, "Failed to generate worksheet content");
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
        }        /// <summary>
                 /// Validate OpenAI configuration
                 /// </summary>
        public async Task<ServiceResult<bool>> ValidateConfigurationAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                // Test the connection with a simple request
                var testMessages = new List<Microsoft.Extensions.AI.ChatMessage>
                {
                    new(Microsoft.Extensions.AI.ChatRole.User, "Test connection. Please respond with 'OK'.")
                };

                var testOptions = new Microsoft.Extensions.AI.ChatOptions
                {
                    MaxOutputTokens = 10,
                    Temperature = 0
                };

                var response = await _chatClient.CompleteAsync(testMessages, testOptions, cancellationToken);

                bool isValid = response?.Message?.Text != null;
                return ServiceResult<bool>.SuccessResult(isValid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OpenAI configuration validation failed");
                return ServiceResult<bool>.ErrorResult($"Configuration validation failed: {ex.Message}");
            }
        }/// <summary>
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
        }

        /// <summary>
        /// Generate the appropriate prompt based on worksheet type and request parameters
        /// </summary>
        private string GeneratePrompt(WorksheetGenerationRequest request)
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
        }        /// <summary>
                 /// Process the AI response and create the final worksheet content result
                 /// </summary>
        private async Task<WorksheetContentResult> ProcessAIResponse(
            string markdownContent,
            WorksheetGenerationRequest request,
            Microsoft.Extensions.AI.UsageDetails? usage,
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
        }        /// <summary>
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
                _logger.LogWarning(ex, "Failed to render markdown to HTML, returning plain text");
                return $"<div class=\"worksheet-content\"><pre>{System.Web.HttpUtility.HtmlEncode(markdownContent)}</pre></div>";
            }
        }

        /// <summary>
        /// Extract questions from the generated content
        /// </summary>
        private List<string> ExtractQuestions(string content)
        {
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
            var titleLine = lines.FirstOrDefault(l => l.Trim().StartsWith("# "));
            return titleLine?.Replace("# ", "").Trim();
        }

        /// <summary>
        /// Calculate estimated duration for worksheet completion
        /// </summary>
        private int CalculateEstimatedDuration(int questionCount, string worksheetType)
        {
            // Base time per question varies by type
            var baseMinutesPerQuestion = worksheetType switch
            {
                "reading-comprehension" => 3,
                "vocabulary" => 2,
                "grammar" => 2,
                "creative-writing" => 5,
                "literary-analysis" => 4,
                _ => 3
            };

            return Math.Max(10, questionCount * baseMinutesPerQuestion);
        }

        /// <summary>
        /// Calculate confidence score based on content quality
        /// </summary>
        private double CalculateConfidenceScore(string content, WorksheetGenerationRequest request)
        {
            double score = 0.5; // Base score

            // Check content length
            if (content.Length > 200) score += 0.1;
            if (content.Length > 500) score += 0.1;

            // Check for proper formatting
            if (content.Contains("#")) score += 0.1; // Has headers
            if (content.Contains("?")) score += 0.1; // Has questions

            // Check if requirements were met
            var extractedQuestions = ExtractQuestions(content);
            if (extractedQuestions.Count >= request.MaxQuestions - 2) score += 0.1;
            if (extractedQuestions.Count <= request.MaxQuestions + 2) score += 0.1;

            return Math.Min(1.0, score);
        }

        /// <summary>
        /// Generate warnings based on content analysis
        /// </summary>
        private List<string> GenerateWarnings(string content, WorksheetGenerationRequest request)
        {
            var warnings = new List<string>();

            if (content.Length < 100)
            {
                warnings.Add("Generated content appears to be shorter than expected");
            }

            var extractedQuestions = ExtractQuestions(content);
            if (extractedQuestions.Count < request.MaxQuestions - 2)
            {
                warnings.Add($"Generated fewer questions ({extractedQuestions.Count}) than requested ({request.MaxQuestions})");
            }

            if (!content.Contains("?"))
            {
                warnings.Add("No question marks found in generated content");
            }

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
