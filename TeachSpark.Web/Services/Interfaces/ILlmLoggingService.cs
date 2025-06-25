using TeachSpark.Web.Services.Models;

namespace TeachSpark.Web.Services.Interfaces
{
    /// <summary>
    /// Service for logging LLM interactions to dedicated log files for debugging and review
    /// </summary>
    public interface ILlmLoggingService
    {
        /// <summary>
        /// Log an LLM request and response to a dedicated file
        /// </summary>
        /// <param name="request">The original worksheet generation request</param>
        /// <param name="response">The response received from the LLM</param>
        /// <param name="metadata">Additional metadata about the call</param>
        /// 
        /// 
        Task LogLlmInteractionAsync(
            WorksheetGenerationRequest request,
            string response,
            LlmCallMetadata metadata);

        /// <summary>
        /// Log an LLM error or failed call
        /// </summary>
        /// <param name="request">The original worksheet generation request</param>
        /// <param name="error">The error that occurred</param>
        /// <param name="metadata">Additional metadata about the call</param>
        /// 
        /// 
        Task LogLlmErrorAsync(
            WorksheetGenerationRequest request,
            Exception error,
            LlmCallMetadata metadata);
    }
}
