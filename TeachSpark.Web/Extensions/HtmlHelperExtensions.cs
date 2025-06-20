using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TeachSpark.Web.Extensions
{
    public static class HtmlHelperExtensions
    {
        /// <summary>
        /// Renders a script tag with the correct asset path from webpack manifest
        /// </summary>
        /// <param name="html">The HTML helper</param>
        /// <param name="logicalPath">The logical path of the script (e.g., "js/site.js")</param>
        /// <param name="htmlAttributes">Additional HTML attributes</param>
        /// <returns>HTML string containing the script tag</returns>
        public static IHtmlContent AssetScript(this IHtmlHelper html, string logicalPath, object? htmlAttributes = null)
        {
            var assetsService = html.ViewContext.HttpContext.RequestServices.GetRequiredService<IAssetsService>();
            var actualPath = assetsService.GetAssetPath(logicalPath);
            var tagBuilder = new TagBuilder("script");
            tagBuilder.Attributes["src"] = actualPath;
            tagBuilder.Attributes["type"] = "text/javascript";

            if (htmlAttributes != null)
            {
                var attributes = new RouteValueDictionary(htmlAttributes);
                tagBuilder.MergeAttributes(attributes);
            }

            return tagBuilder;
        }

        /// <summary>
        /// Renders a link tag for CSS with the correct asset path from webpack manifest
        /// </summary>
        /// <param name="html">The HTML helper</param>
        /// <param name="logicalPath">The logical path of the CSS file (e.g., "css/site.css")</param>
        /// <param name="htmlAttributes">Additional HTML attributes</param>
        /// <returns>HTML string containing the link tag</returns>
        public static IHtmlContent AssetStylesheet(this IHtmlHelper html, string logicalPath, object? htmlAttributes = null)
        {
            var assetsService = html.ViewContext.HttpContext.RequestServices.GetRequiredService<IAssetsService>();
            var actualPath = assetsService.GetAssetPath(logicalPath);
            var tagBuilder = new TagBuilder("link");
            tagBuilder.Attributes["rel"] = "stylesheet";
            tagBuilder.Attributes["href"] = actualPath;

            if (htmlAttributes != null)
            {
                var attributes = new RouteValueDictionary(htmlAttributes);
                tagBuilder.MergeAttributes(attributes);
            }

            return tagBuilder;
        }

        /// <summary>
        /// Gets the actual asset path for use in custom scenarios
        /// </summary>
        /// <param name="html">The HTML helper</param>
        /// <param name="logicalPath">The logical path of the asset</param>
        /// <returns>The actual asset path with content hash</returns>
        public static string AssetPath(this IHtmlHelper html, string logicalPath)
        {
            var assetsService = html.ViewContext.HttpContext.RequestServices.GetRequiredService<IAssetsService>();
            return assetsService.GetAssetPath(logicalPath);
        }

        /// <summary>
        /// Renders multiple script tags for common assets (runtime, vendors, etc.)
        /// </summary>
        /// <param name="html">The HTML helper</param>
        /// <param name="entryPoint">The main entry point (e.g., "site", "validation")</param>
        /// <returns>HTML content with all required script tags</returns>
        public static IHtmlContent AssetScriptBundle(this IHtmlHelper html, string entryPoint)
        {
            var content = new HtmlContentBuilder();

            // Add runtime chunk first (contains webpack runtime)
            content.AppendHtml(html.AssetScript("js/runtime.js"));
            content.AppendLine();

            // Add vendor chunks
            content.AppendHtml(html.AssetScript("js/jquery.js"));
            content.AppendLine();
            content.AppendHtml(html.AssetScript("js/bootstrap.js"));
            content.AppendLine();
            content.AppendHtml(html.AssetScript("js/vendors.js"));
            content.AppendLine();

            // Add the specific entry point
            content.AppendHtml(html.AssetScript($"js/{entryPoint}.js"));

            return content;
        }

        /// <summary>
        /// Renders CSS bundle including vendor and site styles
        /// </summary>
        /// <param name="html">The HTML helper</param>
        /// <param name="includeVendors">Whether to include vendor CSS</param>
        /// <returns>HTML content with CSS link tags</returns>
        public static IHtmlContent AssetStylesheetBundle(this IHtmlHelper html, bool includeVendors = true)
        {
            var content = new HtmlContentBuilder();

            if (includeVendors)
            {
                content.AppendHtml(html.AssetStylesheet("css/vendors.css"));
                content.AppendLine();
            }

            content.AppendHtml(html.AssetStylesheet("css/site.css"));

            return content;
        }
    }
}
