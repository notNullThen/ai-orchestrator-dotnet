#pragma warning disable IDE0210 // Convert to top-level statements
#pragma warning disable IDE0040 // Add accessibility modifiers
namespace AIOrchestrator;

using AIOrchestrator.Support;

sealed class Program
{
    static async Task Main(string[] args)
    {
        if (args.Contains("--start-regular-chat"))
        {
            await new ConversationHandler().ConversationAsync();
            return;
        }

        await new AIManager().StartAsync();
    }
}
