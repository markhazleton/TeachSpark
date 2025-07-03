using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TeachSpark.Web;

namespace TeachSpark.Web.Extensions;

/// <summary>
/// Diagnostic utility for troubleshooting asset loading issues in production
/// </summary>
public static class DiagnosticUtility
{
    /// <summary>
    /// Logs diagnostic information about the current environment and asset configuration
    /// </summary>
    public static void LogDiagnosticInfo(ILogger logger, IWebHostEnvironment environment, IAssetsService assetsService)
    {
        try
        {
            logger.LogInformation("=== Asset Diagnostic Information ===");
            logger.LogInformation("Environment: {Environment}", environment.EnvironmentName);
            logger.LogInformation("WebRootPath: {WebRootPath}", environment.WebRootPath);
            logger.LogInformation("ContentRootPath: {ContentRootPath}", environment.ContentRootPath);

            // Check if manifest file exists
            var manifestPath = Path.Combine(environment.WebRootPath, "assets-manifest.json");
            logger.LogInformation("Manifest Path: {ManifestPath}", manifestPath);
            logger.LogInformation("Manifest Exists: {ManifestExists}", File.Exists(manifestPath));

            if (File.Exists(manifestPath))
            {
                var manifestContent = File.ReadAllText(manifestPath);
                logger.LogInformation("Manifest Content: {ManifestContent}", manifestContent);
            }

            // Test common asset lookups
            var testAssets = new[] { "js/site.js", "/js/site.js", "css/site.css", "/css/site.css" };
            foreach (var asset in testAssets)
            {
                try
                {
                    var resolvedPath = assetsService.GetAssetPath(asset);
                    logger.LogInformation("Asset '{Asset}' resolves to '{ResolvedPath}'", asset, resolvedPath);

                    // Check if the resolved file actually exists
                    var physicalPath = Path.Combine(environment.WebRootPath, resolvedPath.TrimStart('/'));
                    logger.LogInformation("Physical path '{PhysicalPath}' exists: {Exists}", physicalPath, File.Exists(physicalPath));
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error resolving asset '{Asset}'", asset);
                }
            }

            // List actual files in wwwroot
            if (Directory.Exists(environment.WebRootPath))
            {
                var jsDir = Path.Combine(environment.WebRootPath, "js");
                var cssDir = Path.Combine(environment.WebRootPath, "css");

                if (Directory.Exists(jsDir))
                {
                    var jsFiles = Directory.GetFiles(jsDir, "*", SearchOption.TopDirectoryOnly);
                    logger.LogInformation("JS files found: {JsFiles}", string.Join(", ", jsFiles.Select(Path.GetFileName)));
                }

                if (Directory.Exists(cssDir))
                {
                    var cssFiles = Directory.GetFiles(cssDir, "*", SearchOption.TopDirectoryOnly);
                    logger.LogInformation("CSS files found: {CssFiles}", string.Join(", ", cssFiles.Select(Path.GetFileName)));
                }
            }

            logger.LogInformation("=== End Asset Diagnostic Information ===");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during diagnostic logging");
        }
    }
}
