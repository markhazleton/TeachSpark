// Example of how to use the LoggingUtility in Program.cs
// This code should be added once VS Code/Visual Studio recognizes the Serilog packages
// This file is commented out to avoid conflicts with top-level statements in Program.cs

/*
using Microsoft.EntityFrameworkCore;
using TeachSpark.Web;
using TeachSpark.Web.Data;
using TeachSpark.Web.Data.Entities;
using TeachSpark.Web.Services;
using TeachSpark.Web.Services.Extensions;
using TeachSpark.Web.Extensions; // This enables the LoggingUtility extension methods

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog logging early in the startup process (following Azure best practices)
builder.ConfigureSerilogLogging("TeachSpark.Web");

// Alternative usage if extension method doesn't work:
// LoggingUtility.ConfigureSerilogLogging(builder, "TeachSpark.Web");

// ... rest of your existing configuration ...

var app = builder.Build();

// ... your middleware and route configuration ...

try
{
    app.Run();
}
finally
{
    // Ensure Serilog logs are flushed on shutdown
    LoggingUtility.CloseAndFlushLogs();
}
*/
