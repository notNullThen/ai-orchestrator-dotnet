namespace AIOrchestrator.Utilities;

internal sealed class MarkdownProcess
{
    public static string RemoveCodeMarkdown(string markdownString)
    {
        if (string.IsNullOrWhiteSpace(markdownString))
        {
            return string.Empty;
        }

        var cleaned = markdownString.Trim();

        // Check for ```json ... ``` or ``` ... ```
        if (cleaned.Contains("```", StringComparison.Ordinal))
        {
            // Try to extract content between code blocks
            var firstBackticks = cleaned.IndexOf("```", StringComparison.Ordinal);
            var lastBackticks = cleaned.LastIndexOf("```", StringComparison.Ordinal);

            if (firstBackticks != lastBackticks)
            {
                // Move past the first backticks
                var start = firstBackticks + 3;
                // If it's ```json, move past 'json'
                if (
                    cleaned
                        .Substring(start)
                        .StartsWith("json", StringComparison.InvariantCultureIgnoreCase)
                )
                {
                    start += 4;
                }

                int length = lastBackticks - start;
                if (length > 0)
                {
                    cleaned = cleaned.Substring(start, length).Trim();
                }
            }
        }

        // If it still doesn't look like JSON (doesn't start with { and end with }),
        // let's try to find the first { and last }
        if (!cleaned.StartsWith('{') || !cleaned.EndsWith('}'))
        {
            var firstBrace = cleaned.IndexOf('{');
            var lastBrace = cleaned.LastIndexOf('}');

            if (firstBrace >= 0 && lastBrace > firstBrace)
            {
                cleaned = cleaned.Substring(firstBrace, lastBrace - firstBrace + 1);
            }
        }

        return cleaned.Trim();
    }
}
