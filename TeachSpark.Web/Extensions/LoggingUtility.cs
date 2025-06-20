using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;

namespace TeachSpark.Web.Extensions;

/// <summary>
/// Extension class for configuring Serilog logging with file output and appropriate filters.
/// Implements Azure best practices for logging with proper error handling and security considerations.
/// </summary>
public static class LoggingUtility
{    /// <summary>
     /// Configures Serilog logging for the application with file-based output and EF Core filtering.
     /// </summary>
     /// <param name="builder">The WebApplicationBuilder instance</param>
     /// <param name="applicationName">The name of the application for log file naming</param>
     /// <exception cref="ArgumentNullException">Thrown when builder or applicationName is null</exception>
     /// <exception cref="InvalidOperationException">Thrown when log configuration fails</exception>
    public static void ConfigureSerilogLogging(WebApplicationBuilder builder, string applicationName)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrWhiteSpace(applicationName);

        try
        {
            // Get log path from configuration or use default
            string logPath = builder.Configuration.GetValue<string>("TeachSpark:LogFilePath")
                             ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                                           "TeachSpark", "logs", $"{applicationName}-log-.txt");

            // Ensure log directory exists
            var logDirectory = Path.GetDirectoryName(logPath);
            if (!string.IsNullOrEmpty(logDirectory) && !Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            // Clear existing logging providers
            builder.Logging.ClearProviders();

            // Enable Serilog self-log for troubleshooting in Development environment only
            if (builder.Environment.IsDevelopment())
            {
                Serilog.Debugging.SelfLog.Enable(msg => Console.WriteLine($"Serilog: {msg}"));
            }

            // Configure Serilog with enrichers and appropriate filtering
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithProperty("ApplicationName", applicationName)
                .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
                .MinimumLevel.Information() // Set minimum level to Information
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning) // Reduce Microsoft namespace noise
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .WriteTo.Console(restrictedToMinimumLevel: builder.Environment.IsDevelopment() ? LogEventLevel.Debug : LogEventLevel.Information)
                .WriteTo.File(
                    path: logPath,
                    rollingInterval: RollingInterval.Day,
                    restrictedToMinimumLevel: LogEventLevel.Information,
                    retainedFileCountLimit: 30, // Keep 30 days of logs
                    fileSizeLimitBytes: 50 * 1024 * 1024, // 50MB per file
                    rollOnFileSizeLimit: true,
                    shared: true, // Allow multiple processes to write to the same file
                    flushToDiskInterval: TimeSpan.FromSeconds(1))
                .CreateLogger();

            // Add Serilog to the logging providers
            builder.Logging.AddProvider(new SerilogLoggerProvider(Log.Logger));

            // Additional filtering for specific loggers
            builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
            builder.Logging.AddFilter("Microsoft.AspNetCore.Hosting.Diagnostics", LogLevel.Warning);
            builder.Logging.AddFilter("Microsoft.AspNetCore.StaticFiles", LogLevel.Warning);            // Log initial setup confirmation
            Log.Information("Serilog logging configured successfully for {ApplicationName} in {Environment} environment. Log path: {LogPath}",
                applicationName, builder.Environment.EnvironmentName, logPath);
        }
        catch (Exception ex)
        {
            // If Serilog configuration fails, fall back to console logging
            Console.WriteLine($"Failed to configure Serilog: {ex.Message}");
            builder.Logging.AddConsole();
            throw new InvalidOperationException("Failed to configure Serilog logging", ex);
        }
    }

    /// <summary>
    /// Ensures proper cleanup of Serilog logger on application shutdown.
    /// Should be called in Program.cs after app.Run() or in a using statement.
    /// </summary>
    public static void CloseAndFlushLogs()
    {
        Log.CloseAndFlush();
    }
}
