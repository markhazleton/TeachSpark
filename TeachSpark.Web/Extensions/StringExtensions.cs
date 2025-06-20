using System.Globalization;

namespace TeachSpark.Web.Extensions
{
    /// <summary>
    /// String extension methods for UI formatting
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Converts a string to title case using the current culture
        /// </summary>
        /// <param name="input">The input string</param>
        /// <returns>The string converted to title case</returns>
        public static string ToTitleCase(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(input.ToLower());
        }
    }
}
