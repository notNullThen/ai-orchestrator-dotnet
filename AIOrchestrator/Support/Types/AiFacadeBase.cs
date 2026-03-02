namespace AIOrchestrator.Support.Types;

public abstract class AiFacadeBase()
{
#pragma warning disable CA1822 // Mark members as static
    protected void Exit() => AiManager.Exit();
#pragma warning restore CA1822 // Mark members as static

    public abstract string GetDescription();
}
