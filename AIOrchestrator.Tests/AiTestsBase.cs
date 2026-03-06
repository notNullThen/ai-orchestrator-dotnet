namespace AIOrchestratorTests;

using System.Text.Encodings.Web;
using System.Text.Json;
using AIOrchestrator.Core;

public abstract class AiTestsBase
{
    public required TestContext TestContext { get; set; }

    protected AiManager? AiManager { get; set; }

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    [TestCleanup]
    public void LogContextOnFailure()
    {
        if (TestContext.CurrentTestOutcome != UnitTestOutcome.Passed)
        {
            if (AiManager?.ContextHandler?.Context != null)
            {
                var json = JsonSerializer.Serialize(AiManager.ContextHandler.Context, _jsonOptions);

                TestContext.WriteLine("--- TEST FAILED: FUNCTIONS CALL SNAPSHOT ---");
                TestContext.WriteLine(json);
                TestContext.WriteLine("---------------------------------------");
            }
        }
    }
}
