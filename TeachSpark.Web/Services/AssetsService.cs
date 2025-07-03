using System.Text.Json;

namespace TeachSpark.Web
{
    public interface IAssetsService
    {
        string GetAssetPath(string logicalPath);
    }

    public class AssetsService : IAssetsService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<AssetsService> _logger;
        private Dictionary<string, string>? _manifest;
        private readonly object _lock = new object();

        public AssetsService(IWebHostEnvironment environment, ILogger<AssetsService> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        private Dictionary<string, string> GetManifest()
        {
            if (_manifest != null)
                return _manifest;

            lock (_lock)
            {
                if (_manifest != null)
                    return _manifest;

                var manifestPath = Path.Combine(_environment.WebRootPath, "assets-manifest.json");

                if (!File.Exists(manifestPath))
                {
                    _logger.LogWarning("Assets manifest not found at {ManifestPath}. Using fallback paths.", manifestPath);
                    return new Dictionary<string, string>();
                }

                try
                {
                    var json = File.ReadAllText(manifestPath);
                    _manifest = JsonSerializer.Deserialize<Dictionary<string, string>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new Dictionary<string, string>();

                    _logger.LogInformation("Assets manifest loaded successfully from {ManifestPath}", manifestPath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to load assets manifest from {ManifestPath}", manifestPath);
                    _manifest = new Dictionary<string, string>();
                }

                return _manifest;
            }
        }
        public string GetAssetPath(string logicalPath)
        {
            var manifest = GetManifest();

            // Normalize the logical path to have a leading slash for manifest lookup
            var normalizedLogicalPath = logicalPath.StartsWith('/') ? logicalPath : $"/{logicalPath}";
            var logicalPathWithoutSlash = logicalPath.TrimStart('/');

            // Try multiple variations to find the asset in the manifest
            string? actualPath = null;

            // Try with leading slash first
            if (manifest.TryGetValue(normalizedLogicalPath, out actualPath))
            {
                return EnsureCorrectPath(actualPath);
            }

            // Try without leading slash
            if (manifest.TryGetValue(logicalPathWithoutSlash, out actualPath))
            {
                return EnsureCorrectPath(actualPath);
            }

            // Try common variations for backward compatibility
            var variations = new[]
            {
                $"js/{Path.GetFileName(logicalPath)}", // e.g., "site.js" -> "js/site.js"
                $"css/{Path.GetFileName(logicalPath)}", // e.g., "site.css" -> "css/site.css"
                Path.GetFileName(logicalPath), // Just the filename
                logicalPath.Replace("/js/", "").Replace("/css/", ""), // Remove directory prefixes
            };

            foreach (var variation in variations)
            {
                if (manifest.TryGetValue(variation, out actualPath))
                {
                    return EnsureCorrectPath(actualPath);
                }
                if (manifest.TryGetValue($"/{variation}", out actualPath))
                {
                    return EnsureCorrectPath(actualPath);
                }
            }

            // Log all manifest keys for debugging when asset is not found
            _logger.LogWarning("Asset {LogicalPath} not found in manifest. Available keys: {ManifestKeys}",
                logicalPath, string.Join(", ", manifest.Keys));

            // Fallback to the logical path (useful in development)
            return normalizedLogicalPath;
        }

        private string EnsureCorrectPath(string path)
        {
            // Ensure the path starts with a forward slash for web compatibility
            if (!path.StartsWith('/'))
            {
                return $"/{path}";
            }
            return path;
        }
    }
}
