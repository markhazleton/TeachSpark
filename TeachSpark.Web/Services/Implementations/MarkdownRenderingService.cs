using Markdig;
using System.Text.RegularExpressions;

namespace TeachSpark.Web.Services.Implementations
{
    /// <summary>
    /// Service for rendering Markdown content to HTML and extracting worksheet metadata
    /// </summary>
    public class MarkdownRenderingService
    {
        private readonly MarkdownPipeline _pipeline;
        private readonly ILogger<MarkdownRenderingService> _logger;

        public MarkdownRenderingService(ILogger<MarkdownRenderingService> logger)
        {
            _logger = logger;

            // Configure Markdig pipeline with extensions for educational content
            _pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions() // Includes tables, task lists, etc.
                .UseAutoIdentifiers() // Auto-generate IDs for headings
                .UsePipeTables() // Better table support
                .UseTaskLists() // Checkbox support
                .UseEmphasisExtras() // Strikethrough, etc.
                .Build();
        }

        /// <summary>
        /// Convert Markdown to HTML with educational styling
        /// </summary>
        public string RenderToHtml(string markdown)
        {
            if (string.IsNullOrWhiteSpace(markdown))
                return string.Empty;

            try
            {
                var html = Markdown.ToHtml(markdown, _pipeline);

                // Add educational-specific CSS classes
                html = AddEducationalStyling(html);

                return html;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to render Markdown to HTML");
                return $"<div class='alert alert-danger'>Failed to render content: {ex.Message}</div>";
            }
        }

        /// <summary>
        /// Extract metadata from worksheet Markdown
        /// </summary>
        public WorksheetMetadata ExtractMetadata(string markdown)
        {
            var metadata = new WorksheetMetadata();

            if (string.IsNullOrWhiteSpace(markdown))
                return metadata;

            try
            {
                // Extract title (first # heading)
                var titleMatch = Regex.Match(markdown, @"^#\s+(.+)$", RegexOptions.Multiline);
                if (titleMatch.Success)
                    metadata.Title = titleMatch.Groups[1].Value.Trim();

                // Extract estimated time
                var timeMatch = Regex.Match(markdown, @"\*\*Estimated Time:\*\*\s*(.+)$", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                if (timeMatch.Success)
                    metadata.EstimatedTime = timeMatch.Groups[1].Value.Trim();

                // Extract questions (numbered lists under Questions section)
                var questionsSection = ExtractSection(markdown, "Questions");
                if (!string.IsNullOrEmpty(questionsSection))
                {
                    var questionMatches = Regex.Matches(questionsSection, @"^\d+\.\s+", RegexOptions.Multiline);
                    metadata.QuestionCount = questionMatches.Count;

                    // Extract actual question text
                    metadata.Questions = Regex.Matches(questionsSection, @"^\d+\.\s+\*\*.*?\*\*\s+(.+)$", RegexOptions.Multiline)
                        .Cast<Match>()
                        .Select(m => m.Groups[1].Value.Trim())
                        .ToList();
                }

                // Check for answer key
                metadata.HasAnswerKey = markdown.Contains("## Answer Key", StringComparison.OrdinalIgnoreCase);

                // Extract vocabulary words
                var vocabSection = ExtractSection(markdown, "Vocabulary");
                if (!string.IsNullOrEmpty(vocabSection))
                {
                    var vocabMatches = Regex.Matches(vocabSection, @"^\|\s*(.+?)\s*\|", RegexOptions.Multiline);
                    metadata.VocabularyWords = vocabMatches.Cast<Match>()
                        .Skip(1) // Skip header row
                        .Select(m => m.Groups[1].Value.Trim())
                        .Where(w => !string.IsNullOrEmpty(w) && w != "---")
                        .ToList();
                }

                // Extract standards alignment
                var standardsMatch = Regex.Match(markdown, @"\*\*Standards:\*\*\s*(.+)$", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                if (standardsMatch.Success)
                    metadata.Standards = standardsMatch.Groups[1].Value.Trim();

                return metadata;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to extract metadata from Markdown");
                return metadata;
            }
        }

        /// <summary>
        /// Generate a clean, printable version of the worksheet
        /// </summary>
        public string GeneratePrintVersion(string markdown)
        {
            if (string.IsNullOrWhiteSpace(markdown))
                return string.Empty;

            // Remove answer key section for student version
            var studentVersion = Regex.Replace(markdown,
                @"## Answer Key.*?(?=##|\z)",
string.Empty,
                RegexOptions.Singleline | RegexOptions.IgnoreCase);

            // Remove assessment rubric (teacher-only)
            studentVersion = Regex.Replace(studentVersion,
                @"## Assessment Rubric.*?(?=##|\z)",
string.Empty,
                RegexOptions.Singleline | RegexOptions.IgnoreCase);

            // Clean up extra whitespace
            studentVersion = Regex.Replace(studentVersion, @"\n{3,}", "\n\n");

            return studentVersion.Trim();
        }

        /// <summary>
        /// Extract a specific section from Markdown
        /// </summary>
        private string ExtractSection(string markdown, string sectionName)
        {
            var pattern = $@"## {Regex.Escape(sectionName)}(.*?)(?=##|\z)";
            var match = Regex.Match(markdown, pattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);
            return match.Success ? match.Groups[1].Value.Trim() : string.Empty;
        }

        /// <summary>
        /// Add educational-specific CSS classes to HTML
        /// </summary>
        private string AddEducationalStyling(string html)
        {
            // Add classes for tables
            html = html.Replace("<table>", "<table class=\"table table-striped table-hover\">");

            // Add classes for answer sections
            html = Regex.Replace(html, @"<h2>Answer Key</h2>",
                "<h2 class=\"answer-key-header text-primary\"><i class=\"bi bi-key-fill me-2\"></i>Answer Key</h2>");

            // Add classes for question sections
            html = Regex.Replace(html, @"<h3>Part ([ABC]):",
                "<h3 class=\"question-section-header text-secondary\"><i class=\"bi bi-list-ol me-2\"></i>Part $1:");

            // Add classes for instructions
            html = Regex.Replace(html, @"<h2>Instructions</h2>",
                "<h2 class=\"instructions-header text-info\"><i class=\"bi bi-info-circle-fill me-2\"></i>Instructions</h2>");

            return html;
        }
    }

    /// <summary>
    /// Metadata extracted from worksheet Markdown
    /// </summary>
    public class WorksheetMetadata
    {
        public string Title { get; set; } = string.Empty;
        public string EstimatedTime { get; set; } = string.Empty;
        public int QuestionCount { get; set; }
        public List<string> Questions { get; set; } = new();
        public bool HasAnswerKey { get; set; }
        public List<string> VocabularyWords { get; set; } = new();
        public string Standards { get; set; } = string.Empty;
    }
}
