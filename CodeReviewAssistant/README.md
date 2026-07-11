# CodeReviewAssistant — Backend

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-12.0-239120?logo=csharp&logoColor=white)](https://learn.microsoft.com/dotnet/csharp/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-Minimal%20API-512BD4?logo=dotnet&logoColor=white)](https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis)
[![Ollama](https://img.shields.io/badge/Ollama-local-000000?logo=ollama&logoColor=white)](https://ollama.com/)
[![Gemma](https://img.shields.io/badge/Gemma-gemma4%3Ae4b-4285F4?logo=google&logoColor=white)](https://ai.google.dev/gemma)

The **.NET 8 Minimal API** that powers the Code Review Assistant. It accepts code
from the frontend, builds a review prompt, calls a **local Ollama** model
(`gemma4:e4b`), and returns the structured review. It also serves the built React
app from `wwwroot`, so the whole demo runs as a single local process.

> This is the backend of the [Code Review Assistant](../README.md). See the root
> README for the full architecture, diagrams, and demo script.

---

## Requirements

| Tool | Why | Get it |
|------|-----|--------|
| **.NET 8 SDK** | builds & runs the API | https://dotnet.microsoft.com/download |
| **Ollama** | runs Gemma locally, exposes a REST API | https://ollama.com/download |
| **Gemma model** | the reviewing model | `ollama pull gemma4:e4b` |

---

## Run

```bash
# from this folder
dotnet run
```

`dotnet run` prints the local URL (e.g. http://localhost:5099). Open it to use the
UI (once the React app has been built into `wwwroot`), or call the API directly.

Make sure Ollama is running (`ollama serve`) and the model is pulled
(`ollama pull gemma4:e4b`).

---

## API

### `POST /api/review`

Reviews a snippet of code.

**Request body**

```json
{
  "code": "public string GetUser(string id) { ... }",
  "language": "csharp"
}
```

**Response**

```json
{
  "model": "gemma4:e4b",
  "review": "Security: ...\nPerformance: ...\nBest Practices: ...",
  "durationMs": 1234
}
```

---

## Project structure

```
CodeReviewAssistant/
├─ Program.cs                   # composition root: configure → map → run
├─ Endpoints/
│  └─ ReviewEndpoints.cs        # MapReviewEndpoints() → POST /api/review handler
├─ Exceptions/
│  └─ OllamaException.cs        # thrown when Ollama is unreachable or errors
├─ Models/
│  ├─ OllamaGenerateResponse.cs # shape of Ollama's /api/generate response
│  ├─ OllamaOptions.cs          # typed config (BaseUrl, Model)
│  ├─ ReviewRequest.cs          # incoming { code, language }
│  └─ ReviewResult.cs           # outgoing { model, review, durationMs }
├─ Services/
│  ├─ ICodeReviewService.cs     # review abstraction
│  └─ CodeReviewService.cs      # builds prompt, calls Ollama, parses result
├─ appsettings.json             # Ollama base URL + model
└─ wwwroot/                     # the built React app lands here
```

Each concern lives in its own file; `Program.cs` just wires them together. New
features get their own `XyzEndpoints.cs` + `MapXyzEndpoints()`.

---

## Configuration

`appsettings.json`:

```json
"Ollama": { "BaseUrl": "http://localhost:11434", "Model": "gemma4:e4b" }
```

- **Output language** and prompt structure are set in `BuildPrompt()` inside
  `CodeReviewService.cs`.
- `num_ctx` is raised to 8192 there — the default 4K context is too small for real
  code.
- Low `temperature` (0.2) keeps reviews consistent rather than creative.
- Tight on VRAM? Switch to `gemma4:e2b` in `appsettings.json`.

---

## Troubleshooting

| Symptom | Fix |
|---------|-----|
| "Could not reach Ollama" | Start `ollama serve`; confirm http://localhost:11434 responds. |
| "Ollama responded with 404" | Model not pulled: `ollama pull gemma4:e4b`. |
| First response very slow | Normal — model loading into VRAM. Pre-warm before use. |
| Out-of-memory / very slow | Switch to `gemma4:e2b` in `appsettings.json`. |
| Blank page in browser | Build the React app so it lands in `wwwroot`. |
