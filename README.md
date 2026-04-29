# AIOrchestrator

[![NuGet](https://img.shields.io/nuget/v/AIOrchestrator)](https://www.nuget.org/packages/AIOrchestrator)
[![GitHub](https://img.shields.io/badge/github-repo-black.svg)](https://github.com/notNullThen/ai-orchestrator-dotnet)

A powerful .NET library that uses local LLMs for intelligent function orchestration and routing.

### Installation

Install the NuGet package:

```bash
dotnet add package AIOrchestrator
```

### Demo: [AI Time Manager](https://github.com/notNullThen/ai-ollama-time-manager)

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

## Project Overview

- Decides which function to call based on user input
- Main logic: `AIOrchestrator/AiManager.cs`
- Entry point: `AIOrchestrator/Program.cs`
- Functions and their descriptions can be changed in the `Application` folder.
- The flexible—AI approach uses the descriptions to handle and route logic as needed.
- Extend with modules in `AIOrchestrator/Support/`

---
MIT License | Author: [notNullThen](https://github.com/notNullThen)
