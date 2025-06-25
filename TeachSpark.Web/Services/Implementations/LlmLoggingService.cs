using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using TeachSpark.Web.Configuration;
using TeachSpark.Web.Services.Interfaces;
using TeachSpark.Web.Services.Models;

namespace TeachSpark.Web.Services.Implementations
{
    /// <summary>
    /// Service for logging LLM interactions to dedicated files for debugging and review
    /// </summary>
    public class LlmLoggingService : ILlmLoggingService
    {
        private readonly string _logDirectory;
        private readonly ILogger<LlmLoggingService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;
        private static readonly SemaphoreSlim _fileLock = new(1, 1);

        public LlmLoggingService(ILogger<LlmLoggingService> logger)
        {
            _logger = logger;
            _logDirectory = @"c:\websites\teachspark\logs";

            // Configure JSON serialization options for readable output
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };

            // Ensure log directory exists
            EnsureLogDirectoryExists();
        }

        /// <summary>
        /// Log an LLM request and response to a dedicated file
        /// </summary>
        public async Task LogLlmInteractionAsync(
            WorksheetGenerationRequest request,
            string response,
            LlmCallMetadata metadata)
        {
            try
            {
                var timestamp = DateTime.UtcNow;
                var fileName = GenerateFileName(timestamp, "success");
                var filePath = Path.Combine(_logDirectory, fileName);

                var logEntry = new LlmLogEntry
                {
                    Status = "SUCCESS",
                    Timestamp = timestamp,
                    RequestId = metadata.RequestId,
                    Metadata = metadata,
                    Request = request,
                    SystemPrompt = request.Template.SystemPromptTemplate,
                    UserPrompt = request.Template.UserPromptTemplate,
                    Response = response,
                    ErrorDetails = null
                };

                await WriteLogEntryAsync(filePath, logEntry);

                _logger.LogInformation("LLM error logged to file: {FileName}", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log LLM error to file");
            }
        }

        /// <summary>
        /// Log an LLM error or failed call
        /// </summary>
        public async Task LogLlmErrorAsync(
            WorksheetGenerationRequest request,
            Exception error,
            LlmCallMetadata metadata)
        {
            try
            {
                var timestamp = DateTime.UtcNow;
                var fileName = GenerateFileName(timestamp, "error");
                var filePath = Path.Combine(_logDirectory, fileName);

                var logEntry = new LlmLogEntry
                {
                    Status = "ERROR",
                    Timestamp = timestamp,
                    RequestId = metadata.RequestId,
                    Metadata = metadata,
                    Request = request,
                    SystemPrompt = request.Template.SystemPromptTemplate,
                    UserPrompt = request.Template.UserPromptTemplate,
                    Response = null,
                    ErrorDetails = new ErrorDetails
                    {
                        Message = error.Message,
                        StackTrace = error.StackTrace,
                        Type = error.GetType().FullName,
                        InnerException = error.InnerException?.Message
                    }
                };

                await WriteLogEntryAsync(filePath, logEntry);

                _logger.LogInformation("LLM error logged to file: {FileName}", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log LLM error to file");
            }
        }

        /// <summary>
        /// Generate a timestamped filename for the log
        /// </summary>
        private static string GenerateFileName(DateTime timestamp, string status)
        {
            // Format: LLM_YYYYMMDD_HHMMSS_RequestId_Status.json
            return $"LLM_{timestamp:yyyyMMdd}_{timestamp:HHmmss}_{Guid.NewGuid():N}_{status}.json";
        }

        /// <summary>
        /// Ensure the log directory exists
        /// </summary>
        private void EnsureLogDirectoryExists()
        {
            try
            {
                if (!Directory.Exists(_logDirectory))
                {
                    Directory.CreateDirectory(_logDirectory);
                    _logger.LogInformation("Created LLM log directory: {Directory}", _logDirectory);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create LLM log directory: {Directory}", _logDirectory);
                throw;
            }
        }

        /// <summary>
        /// Write the log entry to file with proper locking
        /// </summary>
        private async Task WriteLogEntryAsync(string filePath, LlmLogEntry logEntry)
        {
            await _fileLock.WaitAsync();
            try
            {
                var json = JsonSerializer.Serialize(logEntry, _jsonOptions);
                await File.WriteAllTextAsync(filePath, json, Encoding.UTF8);
            }
            finally
            {
                _fileLock.Release();
            }
        }
    }

    /// <summary>
    /// Complete log entry structure for LLM interactions
    /// </summary>
    public class LlmLogEntry
    {
        public string Status { get; set; } = string.Empty; // SUCCESS or ERROR
        public DateTime Timestamp { get; set; }
        public string RequestId { get; set; } = string.Empty;
        public LlmCallMetadata Metadata { get; set; } = new();
        public WorksheetGenerationRequest Request { get; set; } = new();
        public string SystemPrompt { get; set; } = string.Empty;
        public string UserPrompt { get; set; } = string.Empty;
        public string? Response { get; set; }
        public ErrorDetails? ErrorDetails { get; set; }
    }

    /// <summary>
    /// Error details for failed LLM calls
    /// </summary>
    public class ErrorDetails
    {
        public string Message { get; set; } = string.Empty;
        public string? StackTrace { get; set; }
        public string? Type { get; set; }
        public string? InnerException { get; set; }
    }
}
