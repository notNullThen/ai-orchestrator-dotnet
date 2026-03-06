namespace AIOrchestrator.Utilities;

internal sealed class MarkdownProcess
{
    public static string RemoveCodeMarkdown(string markdownString)
    {
        var cleaned = markdownString.Trim();

        if (cleaned.StartsWith("```json", StringComparison.InvariantCulture))
        {
            cleaned = cleaned.Substring(7);
        }
        else if (cleaned.StartsWith("```", StringComparison.InvariantCulture))
        {
            cleaned = cleaned.Substring(3);
        }

        if (cleaned.EndsWith("```", StringComparison.InvariantCulture))
        {
            cleaned = cleaned[..^3];
        }

        return cleaned.Trim();
    }
}
