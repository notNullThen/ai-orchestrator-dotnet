# AIOrchestratorDotNET NuGet package

### It is used as a NuGet package in [Time Calculator AI Powered](https://github.com/notNullThen/time-calculator-blazor) project

## Local LLM Support

This solution uses [Ollama](https://ollama.com/) to run fully local large language models (LLMs).

This project takes the function-calling decision on an LLM AI model:

- Given a user input, it selects and runs the appropriate functions.
- Decides the order and number of functions to be called.
- Decides when to stop the processing and return results.
- Considers the context automatically.

This enables seamless, intuitive, human-decision-based orchestration of logic in .NET applications.

## Simple prompt testing

Simple tests are implemetned to test the final result and AI management of prompt and picked model.

## Quick Start

1. **Install [.NET 10.0 SDK](https://dotnet.microsoft.com/download)**
2. **Clone and enter the repo:**

   ```bash
   git clone https://github.com/notNullThen/AIOrchestratorDotNET.git
   cd AIOrchestratorDotNET
   ```

3. **Run the app (with debug details):**

   ```bash
   dotnet run  --project AIOrchestrator/AIOrchestrator.csproj --debug
   ```

## Project Overview

- Decides which function to call based on user input
- Main logic: `AIOrchestrator/AiManager.cs`
- Entry point: `AIOrchestrator/Program.cs`
- Functions and their descriptions can be changed in the `Application` folder.
- The flexible—AI approach uses the descriptions to handle and route logic as needed.
- Extend with modules in `AIOrchestrator/Support/`

---
MIT License | Author: [notNullThen](https://github.com/notNullThen)
