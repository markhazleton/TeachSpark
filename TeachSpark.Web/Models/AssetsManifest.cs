using System.Text.Json.Serialization;

namespace TeachSpark.Web.Services;

/// <summary>
/// Model representing the webpack assets manifest
/// </summary>
public class AssetsManifest
{
    [JsonPropertyName("site.js")]
    public string? SiteJs { get; set; }

    [JsonPropertyName("validation.js")]
    public string? ValidationJs { get; set; }

    [JsonPropertyName("site.css")]
    public string? SiteCss { get; set; }

    [JsonPropertyName("js/runtime.js")]
    public string? RuntimeJs { get; set; }

    [JsonPropertyName("js/vendors.js")]
    public string? VendorsJs { get; set; }

    [JsonPropertyName("js/bootstrap.js")]
    public string? BootstrapJs { get; set; }

    [JsonPropertyName("js/jquery.js")]
    public string? JqueryJs { get; set; }

    [JsonPropertyName("css/vendors.css")]
    public string? VendorsCss { get; set; }

    // Dictionary to handle any additional assets dynamically
    [JsonExtensionData]
    public Dictionary<string, object>? AdditionalAssets { get; set; }
}
