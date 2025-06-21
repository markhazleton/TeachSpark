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

            // Try to get the asset from the manifest
            if (manifest.TryGetValue(normalizedLogicalPath, out var actualPath))
            {
                return actualPath.StartsWith('/') ? actualPath : $"/{actualPath}";
            }

            // Try without leading slash as fallback
            var logicalPathWithoutSlash = logicalPath.TrimStart('/');
            if (manifest.TryGetValue(logicalPathWithoutSlash, out var actualPathFallback))
            {
                return actualPathFallback.StartsWith('/') ? actualPathFallback : $"/{actualPathFallback}";
            }

            // Fallback to the logical path (useful in development)
            _logger.LogDebug("Asset {LogicalPath} not found in manifest, using fallback", logicalPath);
            return normalizedLogicalPath;
        }
    }
}
