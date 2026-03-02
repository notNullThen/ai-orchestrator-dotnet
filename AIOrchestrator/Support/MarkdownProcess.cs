namespace AIOrchestrator.Support;

public class MarkdownProcess
{
    public static string RemoveCodeMarkdown(string markdownString) =>
        markdownString.TrimStart('`', 'j', 's', 'o', 'n', '\n').TrimEnd('`');
}
