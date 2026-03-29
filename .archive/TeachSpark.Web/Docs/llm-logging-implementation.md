# LLM Logging Test Documentation

## Overview

I've successfully created a comprehensive LLM logging system for your TeachSpark application that captures all LLM interactions to dedicated log files in `c:\websites\teachspark\logs`.

## Features Implemented

### 1. LLM Logging Service (`ILlmLoggingService`)

- **Interface**: `ILlmLoggingService` with methods for logging successful calls and errors
- **Implementation**: `LlmLoggingService` that writes detailed JSON log files

### 2. Log File Structure

- **Location**: `c:\websites\teachspark\logs`
- **Naming**: `LLM_YYYYMMDD_HHMMSS_RequestId_Status.json`
- **Examples**:
  - `LLM_20250620_143052_a1b2c3d4e5f6_success.json`
  - `LLM_20250620_143152_f6e5d4c3b2a1_error.json`

### 3. Logged Information

Each log file contains:

- **Request Details**: User ID, email, timestamp, request ID
- **Prompts**: Complete system prompt and user prompt sent to LLM
- **Response**: Full text response from the LLM
- **Metadata**: Model used, token count, cost, duration, temperature settings
- **User Context**: Original worksheet generation request
- **Error Details**: Stack traces and error messages (for failed calls)

### 4. JSON Structure Example

```json
{
  "status": "SUCCESS",
  "timestamp": "2025-06-20T14:30:52.123Z",
  "requestId": "a1b2c3d4-e5f6-7890-1234-567890abcdef",
  "metadata": {
    "userId": "user123",
    "userEmail": "teacher@school.edu",
    "modelUsed": "gpt-4o-mini",
    "duration": "00:00:03.456",
    "tokensUsed": 1250,
    "cost": 0.0125,
    "temperature": 0.7,
    "maxTokens": 2000,
    "timestamp": "2025-06-20T14:30:52.123Z",
    "requestId": "a1b2c3d4-e5f6-7890-1234-567890abcdef"
  },
  "request": {
    "sourceText": "The complete source text used for worksheet generation...",
    "worksheetType": "reading-comprehension",
    "difficultyLevel": "standard",
    "maxQuestions": 10,
    "includeAnswerKey": true,
    "customInstructions": "Focus on vocabulary in context"
  },
  "systemPrompt": "You are an expert educational content creator specializing in creating high-quality worksheets for middle school students...",
  "userPrompt": "Create a reading comprehension worksheet based on the provided text...",
  "response": "# Reading Comprehension Worksheet\n\n## Questions\n1. What is the main idea of the passage?\n2. ...",
  "errorDetails": null
}
```

## Integration Points

### 1. OpenAI LLM Service Enhancement

- Modified `OpenAILlmService` to inject and use `ILlmLoggingService`
- Added user context parameters (userId, userEmail) to the service method
- Logs every successful LLM call and every error automatically

### 2. Controller Updates

- Updated `WorksheetGeneratorController` to pass user information to the LLM service
- Extracts user ID and email from authentication claims

### 3. Dependency Injection

- Registered `ILlmLoggingService` and `LlmLoggingService` in `ServiceCollectionExtensions`
- Service is automatically available throughout the application

## Security & Privacy Considerations

### ✅ What's Logged (Safe for Review)

- Complete prompts and responses for debugging
- User identification for audit trails
- Performance metrics and costs
- Error details for troubleshooting

### ⚠️ Privacy Notes

- Log files contain complete user inputs and AI responses
- Store sensitive educational content temporarily
- Consider implementing log rotation and retention policies
- Ensure log directory access is restricted to authorized personnel

## Usage Examples

### Reviewing a Successful Generation

```bash
# Navigate to log directory
cd c:\websites\teachspark\logs

# Find recent successful generations
ls *_success.json | Sort-Object CreationTime -Descending | Select-Object -First 5

# View a specific log file
Get-Content "LLM_20250620_143052_a1b2c3d4e5f6_success.json" | ConvertFrom-Json | ConvertTo-Json -Depth 10
```

### Debugging Failed Generations

```bash
# Find recent errors
ls *_error.json | Sort-Object CreationTime -Descending | Select-Object -First 5

# View error details
Get-Content "LLM_20250620_143152_f6e5d4c3b2a1_error.json" | ConvertFrom-Json | Select-Object -ExpandProperty errorDetails
```

## Maintenance Tasks

### Log Cleanup Script (Recommended)

Create a scheduled task to clean up old logs:

```powershell
# Delete logs older than 30 days
$logPath = "c:\websites\teachspark\logs"
$cutoffDate = (Get-Date).AddDays(-30)
Get-ChildItem -Path $logPath -Filter "LLM_*.json" | Where-Object { $_.CreationTime -lt $cutoffDate } | Remove-Item
```

### Log Analysis

You can analyze patterns:

- Most common worksheet types requested
- Average token usage and costs
- User activity patterns
- Common error scenarios

## Next Steps

1. **Test the Logging**: Generate a worksheet to create your first log file
2. **Set Up Log Rotation**: Implement automated cleanup of old log files
3. **Create Monitoring**: Set up alerts for high error rates or unusual costs
4. **Analysis Tools**: Consider creating reports from the log data

The logging system is now fully integrated and will automatically capture all LLM interactions for review and debugging purposes!
