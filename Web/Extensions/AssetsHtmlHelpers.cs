using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using TeachSpark.Web.Services;

namespace TeachSpark.Web.Extensions;

/// <summary>
/// HTML helper extensions for rendering webpack assets
/// </summary>
public static class AssetsHtmlHelpers
{
    /// <summary>
    /// Renders a script tag for the specified asset
    /// </summary>
    /// <param name="htmlHelper">The HTML helper</param>
    /// <param name="logicalPath">The logical path of the asset (e.g., "js/site.js")</param>
    /// <returns>HTML string containing the script tag</returns>
    public static IHtmlContent AssetScript(this IHtmlHelper htmlHelper, string logicalPath)
    {
        var assetsService = htmlHelper.ViewContext.HttpContext.RequestServices.GetRequiredService<IAssetsService>();
        var actualPath = assetsService.GetAssetPath(logicalPath);

        var urlHelper = htmlHelper.ViewContext.HttpContext.RequestServices.GetRequiredService<IUrlHelper>();
        var url = urlHelper.Content($"~/{actualPath}");

        return new HtmlString($"<script src=\"{url}\"></script>");
    }

    /// <summary>
    /// Renders a link tag for the specified CSS asset
    /// </summary>
    /// <param name="htmlHelper">The HTML helper</param>
    /// <param name="logicalPath">The logical path of the asset (e.g., "css/site.css")</param>
    /// <returns>HTML string containing the link tag</returns>
    public static IHtmlContent AssetStylesheet(this IHtmlHelper htmlHelper, string logicalPath)
    {
        var assetsService = htmlHelper.ViewContext.HttpContext.RequestServices.GetRequiredService<IAssetsService>();
        var actualPath = assetsService.GetAssetPath(logicalPath);

        var urlHelper = htmlHelper.ViewContext.HttpContext.RequestServices.GetRequiredService<IUrlHelper>();
        var url = urlHelper.Content($"~/{actualPath}");

        return new HtmlString($"<link rel=\"stylesheet\" href=\"{url}\" />");
    }
}
