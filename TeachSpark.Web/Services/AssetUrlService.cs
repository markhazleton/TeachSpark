using Microsoft.AspNetCore.Html;

namespace TeachSpark.Web.Services
{
    public interface IAssetUrlService
    {
        string GetAssetUrl(string logicalPath);
        IHtmlContent GetScriptTag(string logicalPath, object? htmlAttributes = null);
        IHtmlContent GetStylesheetTag(string logicalPath, object? htmlAttributes = null);
    }

    public class AssetUrlService : IAssetUrlService
    {
        private readonly IAssetsService _assetsService;

        public AssetUrlService(IAssetsService assetsService)
        {
            _assetsService = assetsService;
        }

        public string GetAssetUrl(string logicalPath)
        {
            return _assetsService.GetAssetPath(logicalPath);
        }

        public IHtmlContent GetScriptTag(string logicalPath, object? htmlAttributes = null)
        {
            var assetPath = _assetsService.GetAssetPath(logicalPath);

            var attributes = string.Empty;
            if (htmlAttributes != null)
            {
                var props = htmlAttributes.GetType().GetProperties();
                foreach (var prop in props)
                {
                    var name = prop.Name.Replace("_", "-");
                    var value = prop.GetValue(htmlAttributes);
                    attributes += $" {name}=\"{value}\"";
                }
            }

            return new HtmlString($"<script src=\"{assetPath}\"{attributes}></script>");
        }

        public IHtmlContent GetStylesheetTag(string logicalPath, object? htmlAttributes = null)
        {
            var assetPath = _assetsService.GetAssetPath(logicalPath);

            var attributes = string.Empty;
            if (htmlAttributes != null)
            {
                var props = htmlAttributes.GetType().GetProperties();
                foreach (var prop in props)
                {
                    var name = prop.Name.Replace("_", "-");
                    var value = prop.GetValue(htmlAttributes);
                    attributes += $" {name}=\"{value}\"";
                }
            }

            return new HtmlString($"<link rel=\"stylesheet\" href=\"{assetPath}\"{attributes}>");
        }
    }
}
