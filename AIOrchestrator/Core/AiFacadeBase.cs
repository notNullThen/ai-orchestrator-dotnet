namespace AIOrchestrator.Core;

using System;
using AIOrchestrator.Core.AiFacade;

public abstract class AiFacadeBase()
{
    public Action? OnExit { get; set; }

    protected void Exit()
    {
        if (OnExit == null)
        {
            throw new Exception(
                $"The {nameof(OnExit)} action is not set. Set it in {nameof(AiManager)} class."
            );
        }

        OnExit.Invoke();
    }

    public abstract AppDescription GetDescription();
}
