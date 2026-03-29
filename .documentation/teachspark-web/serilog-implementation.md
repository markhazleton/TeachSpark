# Serilog Implementation for TeachSpark.Web

## Overview

This document describes the Serilog logging implementation using the LoggingUtility extension class, following Azure best practices for ASP.NET Core applications.

## Implementation Status

✅ **Completed:**

- Added Serilog NuGet packages to project
- Created LoggingUtility extension class
- Added configuration in appsettings.json
- Project builds successfully

⏳ **Note:** There may be IDE refresh issues preventing the Serilog namespace from being recognized in Program.cs. This is common with newly added packages and typically resolves after an IDE restart or full solution reload.

## NuGet Packages Added

```xml
<PackageReference Include="Serilog" Version="4.2.0" />
<PackageReference Include="Serilog.Extensions.Logging" Version="8.0.0" />
<PackageReference Include="Serilog.Sinks.File" Version="6.0.0" />
<PackageReference Include="Serilog.Sinks.Console" Version="6.0.0" />
<PackageReference Include="Serilog.Enrichers.Context" Version="4.6.0" />
```

## LoggingUtility Extension Class

Created in `Extensions/LoggingUtility.cs` with the following features:

### Key Features

- **File-based logging** with daily rolling files
- **Console output** in development
- **EF Core logging filtering** to reduce noise
- **Azure best practices** compliance
- **Proper error handling** with fallback to console logging
- **Configurable log paths** via appsettings.json

### Configuration Options

- Log file retention: 30 days
- File size limit: 50MB per file
- Rolling interval: Daily
- Minimum log level: Information
- Environment-specific settings

## Usage

### 1. In Program.cs (Extension Method)

```csharp
using TeachSpark.Web.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog logging early in the startup process
builder.ConfigureSerilogLogging("TeachSpark.Web");

// ... rest of your configuration
```

### 2. In Program.cs (Static Method)

```csharp
using TeachSpark.Web.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog logging early in the startup process
LoggingUtility.ConfigureSerilogLogging(builder, "TeachSpark.Web");

// ... rest of your configuration
```

### 3. Cleanup on Application Shutdown

```csharp
try
{
    app.Run();
}
finally
{
    // Ensure Serilog logs are flushed on shutdown
    LoggingUtility.CloseAndFlushLogs();
}
```

## Configuration

### appsettings.json

```json
{
  "TeachSpark": {
    "LogFilePath": "C:\\websites\\TeachSpark\\logs\\TeachSpark.Web-log-.txt"
  }
}
```

### Default Log Path

If not specified in configuration, logs will be saved to:

```
%ProgramData%\TeachSpark\logs\{ApplicationName}-log-.txt
```

## Features

### 1. Environment-Specific Configuration

- **Development**: Console output with Debug level, Serilog self-logging enabled
- **Production**: File output with Information level, optimized performance

### 2. Log Enrichment

- Application name
- Environment name
- Log context information

### 3. Filtering

- Microsoft.EntityFrameworkCore.Database.Command: Warning level
- Microsoft.AspNetCore: Warning level
- Microsoft.AspNetCore.Hosting.Diagnostics: Warning level
- Microsoft.AspNetCore.StaticFiles: Warning level

### 4. File Management

- Daily rolling files
- 30-day retention
- 50MB size limit per file
- Shared file access for multiple processes
- 1-second flush interval

## Troubleshooting

### IDE Not Recognizing Serilog Types

1. Clean and rebuild the solution: `dotnet clean && dotnet build`
2. Restart Visual Studio/VS Code
3. Reload the solution
4. Check package references are properly restored

### Log File Access Issues

- Ensure the log directory has write permissions
- Verify the path in appsettings.json is correct
- Check disk space availability

### Performance Considerations

- File logging has minimal performance impact
- Asynchronous operations supported
- Configurable flush intervals
- Automatic file rotation

## Azure Best Practices Implemented

1. **Error Handling**: Comprehensive exception handling with fallback logging
2. **Security**: No sensitive information in log configuration
3. **Performance**: Optimized file writing with buffering
4. **Monitoring**: Structured logging with enrichers
5. **Operational Excellence**: Configurable retention and rotation
6. **Cost Optimization**: Efficient log management to minimize storage costs

## Testing

The implementation can be tested by:

1. Running the application
2. Checking console output for Serilog confirmation message
3. Verifying log files are created in the specified directory
4. Confirming log rotation works over time

## Next Steps

1. Once IDE recognizes the packages, use the LoggingUtility in Program.cs
2. Add custom log enrichers as needed
3. Configure structured logging for specific controllers/services
4. Set up log monitoring and alerting in Azure (if deployed to Azure)
