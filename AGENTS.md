# AIOrchestrator Repository Guide

## Purpose

AIOrchestrator is a small .NET 10 class library for running local, Ollama-backed agent loops. An application exposes ordinary C# methods through an `AiAppFacadeBase`; the library asks an LLM for JSON function calls, invokes those methods through reflection, records the results, and repeats until the facade's inherited `Exit` method is called.

Treat the checked-in source as the contract of record. The README is introductory and can temporarily lag public API changes. For runtime design details, read [docs/PROJECT_ARCHITECTURE.md](docs/PROJECT_ARCHITECTURE.md) when working on orchestration, prompts, tool descriptions, deserialization, reflection, Ollama transport, or public APIs. It is usually unnecessary for a small documentation, packaging, or isolated model-type edit.

Keep this file concise. When a change invalidates an architectural statement here or in the detailed architecture document, update the affected documentation in the same change.

## Repository Map

- `AIOrchestrator/`: packable library; no external runtime package dependencies.
- `AIOrchestrator/Core/AiManager.cs`: conversation lifecycle and management-prompt construction.
- `AIOrchestrator/Core/ReflectiveRecovery.cs`: optional error analysis, `Exit` fallback, and Markdown constraint storage.
- `AIOrchestrator/Core/AiAppFacade/`: application-facing facade base and function-description models.
- `AIOrchestrator/Core/FunctionsDeserializer*.cs`: extracts one or more JSON calls from model output.
- `AIOrchestrator/Core/MethodInvoker.cs`: reflection lookup and string-to-parameter conversion.
- `AIOrchestrator/Core/ContextHandler.cs`: in-memory function-result history serialized back into the prompt.
- `AIOrchestrator/Core/ErrorHandler.cs`: enriches failures with model, user input, latest model output, and history.
- `AIOrchestrator/OllamaClient/`: internal `/api/generate` client plus the public model-listing helper.
- `AIOrchestrator.Tests/`: MSTest unit tests. Tests are intentionally parallelized with one worker.
- `.github/workflows/publish.yml`: packs and publishes on `v*` tags using the tag as package version.

## Core Runtime Flow

1. A consumer derives from `AiAppFacadeBase(bool multipleFunctionsAtOneResponse)` and implements `GetDescription()` and `GetConstraints()`.
2. `AiManager.StartAsync` stores the user input, resets the exit flag, wires `appInstance.OnExit` to the manager, and enters `ConversationAsync`.
3. Every loop iteration rebuilds the management prompt from the facade description, constraints, user input, and the full recorded context.
4. The internal Ollama client sends a non-streaming `POST /api/generate` request.
5. `FunctionsDeserializer` extracts adjacent function-call JSON objects and deserializes them case-insensitively.
6. `MethodInvoker` finds each named method on the facade, converts positional string arguments with `TypeDescriptor`, and invokes it synchronously.
7. The call and returned value are appended as a `FunctionCallResponse`; the loop continues until `Exit` sets the exit flag.

The model-facing call shape is:

```json
{
  "Function": "MethodName",
  "Parameters": ["first positional value", "second positional value"]
}
```

## Important Contracts

- Function parameters are positional strings, not named JSON values. Keep `FunctionDescription.Parameters` in the same order as the C# method signature.
- Every `FunctionDescription` requires `Name`, `Description`, and `Parameters`; use an empty list for a parameterless method.
- Describe `Exit` to the model. It is inherited from `AiAppFacadeBase` and is how a normal conversation terminates.
- Tool methods should currently be synchronous and return values that `System.Text.Json` can serialize. Reflection does not await `Task`/`ValueTask` results.
- Method lookup uses the exact emitted name and includes public instance/static methods. Avoid overloads for exposed tool names.
- `multipleFunctionsAtOneResponse` changes prompt policy only. Returned calls are still executed sequentially in response order.
- Preserve cancellation-token propagation through the manager and Ollama HTTP calls.
- This is a published NuGet library. Treat public types and signatures as compatibility-sensitive.

## Current Implementation Caveats

- Reflective recovery is opt-in through the `AiManager` constructor's `healingConstraintsFilePath`. Any non-cancellation request or invocation error is sent to the recovery prompt. A plain-text rule is added to the management prompt; a parsed `Exit` JSON function call invokes the facade's `Exit` method. An optional event announces analysis, and an async callback can pause after a new constraint is saved.
- A manager's `ContextHandler` is not cleared by `StartAsync`; reusing the same manager carries earlier function history into later runs.
- If a multi-call response contains `Exit`, later calls in that same parsed response are still invoked because the `foreach` is not stopped.
- Function extraction is regex-delimited rather than a general JSON stream parser. Output without a recognized function object produces an empty list and another loop iteration; nested JSON or delimiter-like text inside values is fragile.
- Missing arguments become `string.Empty` before conversion. Extra arguments throw. Optional/default C# parameter values are not honored.
- `ContextHandler.OnContextUpdated` receives the mutable backing `List<T>`, although the public `Context` property is read-only.

## Build, Test, and Format

Run from the repository root:

```bash
dotnet restore AIOrchestrator.sln
dotnet build AIOrchestrator.sln --configuration Release
dotnet test AIOrchestrator.sln --configuration Release
```

For a local NuGet publish, do not add or change a version in `AIOrchestrator.csproj`. Choose the package version from the latest tag and local feed contents, then pass it to `dotnet pack`:

```bash
dotnet pack AIOrchestrator/AIOrchestrator.csproj --configuration Release --output <local-feed-directory> -p:PackageVersion=<version>
```

Check `dotnet nuget list source` for the registered local feed path. The tag-driven GitHub workflow sets `PackageVersion` separately for public releases.

Formatting is configured through the local CSharpier tool:

```bash
dotnet tool restore
dotnet csharpier format .
```

Prefer focused unit tests that do not require a running Ollama instance. Changes to the end-to-end manager loop should use a bounded fake/loopback HTTP server or another deterministic seam and must prove termination. Add regression tests for both single-call and multi-call behavior when modifying the prompt/parser boundary.
