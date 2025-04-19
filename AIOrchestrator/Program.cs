#pragma warning disable IDE0210 // Convert to top-level statements
#pragma warning disable IDE0040 // Add accessibility modifiers
namespace AIOrchestrator;

using AIOrchestrator.Support;

sealed class Program
{
    private static readonly AIManager _aiManager = new();

    static async Task Main(string[] args)
    {
        if (args.Contains("--debug"))
        {
            _aiManager.Debug = true;
        }
        if (args.Contains("--start-regular-chat"))
        {
            var conversationHandler = new ConversationHandler();
            conversationHandler.SetModel("gemma3");
            await conversationHandler.ConversationAsync();
            return;
        }

        await _aiManager.StartAsync();
    }
}
