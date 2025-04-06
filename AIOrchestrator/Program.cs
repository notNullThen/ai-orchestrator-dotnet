#pragma warning disable IDE0060 // Remove unused parameter

namespace AIOrchestrator;

internal sealed class Program
{
    private static async Task Main(string[] args) => await new AIManager().StartChatAsync();
}
