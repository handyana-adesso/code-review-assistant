# Code Review Assistant — Live Demo (Gemma 4, local)

A tiny, **fully local** AI-augmented app for the knowledge-sharing session.
You paste code → a local Gemma 4 model reviews it for security, performance and
best practices. No cloud, no API key, no per-request cost — the code never
leaves the machine.

```
React + Vite (TypeScript)  →  .NET Minimal API (C#)  →  Ollama (gemma4:e4b)
        src/App.tsx              /api/review            localhost:11434
```

For the talk, the React app is **built into the .NET app's `wwwroot`**, so the
whole thing still runs as a single local process — see the two workflows below.

---

## 0. Prerequisites (one-time)

| Tool | Why | Get it |
|------|-----|--------|
| **Ollama** | runs Gemma locally, exposes a REST API | https://ollama.com/download |
| **.NET 8 SDK** | builds & runs the backend | https://dotnet.microsoft.com/download |
| **Node.js (LTS)** | builds the React frontend | https://nodejs.org |

---

## 1. Pull the model (one-time, ~4.5 GB)

```bash
ollama pull gemma4:e4b
```

> Tight on VRAM? Use `gemma4:e2b` (~2.9 GB) and set `"Model": "gemma4:e2b"` in
> `appsettings.json`. Smoke test: `ollama run gemma4:e4b "Say hello."`

---

## 2. Architecture

### 2A. Architecture overview

```mermaid
flowchart LR
    Dev([Developer]) -->|paste code| UI
    subgraph LOCAL["🔒 Your machine — nothing leaves it"]
        direction LR
        UI["React + Vite UI(TypeScript)"] -->|"POST /api/review"| API[".NET 8 Minimal API"]
        API --> SVC["CodeReviewService"]
        SVC -->|"POST /api/generate"| OLL[("Ollama · gemma4:e4b")]
    end
    OLL -.->|review JSON| UI
```

### 2B. Request lifecycle (sequence)

```mermaid
sequenceDiagram
    actor User
    participant UI as React UI
    participant API as .NET Minimal API
    participant SVC as CodeReviewService
    participant OLL as Ollama (gemma4:e4b)

    User->>UI: Paste code, click "Run review"
    UI->>API: POST /api/review { code, language }
    API->>SVC: ReviewAsync(request)
    SVC->>OLL: POST /api/generate (prompt, num_ctx, temperature)
    Note over OLL: Loads model into VRAM (first call)then generates tokens one by one
    OLL-->>SVC: { response, total_duration }
    SVC-->>API: ReviewResult
    API-->>UI: 200 OK { model, review, durationMs }
    UI-->>User: Render the review
    Note over UI,OLL: All on localhost — nothing leaves the machine
```

### 2C. Backend class diagram

```mermaid
classDiagram
    class ReviewEndpoints {
        <>
        +MapReviewEndpoints(IEndpointRouteBuilder) IEndpointRouteBuilder
        -HandleReview(ReviewRequest, ICodeReviewService, CancellationToken) Task~IResult~
    }
    class ICodeReviewService {
        <>
        +ReviewAsync(ReviewRequest, CancellationToken) Task~ReviewResult~
    }
    class CodeReviewService {
        -OllamaOptions _options
        -HttpClient _http
        +ReviewAsync(ReviewRequest, CancellationToken) Task~ReviewResult~
        -BuildPrompt(ReviewRequest) string
    }
    class OllamaOptions {
        +string BaseUrl
        +string Model
    }
    class ReviewRequest {
        <>
        +string Code
        +string Language
    }
    class ReviewResult {
        <>
        +string Model
        +string Review
        +long DurationMs
    }
    class OllamaGenerateResponse {
        +string Response
        +long TotalDuration
    }
    class OllamaException {
        <>
    }

    CodeReviewService ..|> ICodeReviewService : implements
    ReviewEndpoints ..> ICodeReviewService : calls
    CodeReviewService ..> OllamaOptions : reads
    CodeReviewService ..> ReviewRequest : consumes
    CodeReviewService ..> ReviewResult : returns
    CodeReviewService ..> OllamaGenerateResponse : parses
    CodeReviewService ..> OllamaException : throws
```

---

## 3. Project structure

```
CodeReviewAssistant/            # .NET 8 backend
├─ Program.cs                   # thin composition root: configure → map → run
├─ Endpoints
|  └─ ReviewEndpoints.cs           # MapReviewEndpoints() → POST /api/review handler
├─ Exceptions
|  └─ OllamaException.cs
├─ Models
|  ├─ OllamaGenerateResponse.cs
|  ├─ OllamaOptions.cs             # typed config (BaseUrl, Model)
|  ├─ ReviewRequest.cs
|  └─ ReviewResult.cs
├─ Services
|  ├─ ICodeReviewService.cs
|  └─ CodeReviewService.cs      # builds the prompt, calls Ollama, parses the result
├─ appsettings.json             # Ollama base URL + model
└─ wwwroot/                     # the built React app lands here (step 3A)

react-frontend/                 # React + Vite (TypeScript) UI
├─ src/App.tsx                  # the single-screen app: code in → review out
├─ src/main.tsx
├─ src/index.css
└─ vite.config.ts               # builds into ../CodeReviewAssistant/wwwroot; dev proxy for /api

sample-code/
└─ BadExample.cs                # flawed code to paste during the demo
```

The browser only ever calls **your** endpoint, `/api/review`. The backend then
calls **Ollama's** endpoint, `/api/generate` — that hop is server-side and local.

---

## 4. Running it

### 4A. For the talk (reliable — one process)

Build the React app into `wwwroot`, then run only the backend:

```bash
cd react-frontend
npm install            # first time only
npm run build          # outputs into ../CodeReviewAssistant/wwwroot

cd ../CodeReviewAssistant
dotnet run             # serves the UI + the API from one origin
```

Then open the URL `dotnet run` prints (e.g. http://localhost:5xxx). Two local
processes total: `ollama serve` (usually already running) and `dotnet run`.
No CORS, no dev server — this is what to present from.

### 4B. While developing (hot reload)

Run three processes; Vite proxies `/api` to the backend so there's **no CORS**:

```bash
ollama serve                      # terminal 1
cd CodeReviewAssistant && dotnet run   # terminal 2  (note the http port)
cd react-frontend && npm run dev       # terminal 3  (open the Vite URL, :5173)
```

Set the proxy target in `vite.config.ts` (`server.proxy["/api"]`) to the http
port `dotnet run` prints.

> `npm run build` empties `wwwroot` (`emptyOutDir: true`). Keep a copy of the
> original `index.html` if you want to switch back to the no-build version.

---

## 5. What to show on stage

1. Click **Load example** — a deliberately flawed C# method
   (`sample-code/BadExample.cs`: SQL injection, undisposed connection, no null check).
2. Click **Run review** → Gemma returns four sections: Security / Performance /
   Best Practices / Concrete suggestion.
3. **Prove it stays local.** Two options, strongest first:
   - **Turn off Wi-Fi**, then run a review. It still works — the most visceral
     proof there's no cloud call.
   - Open Chrome **DevTools → Network** (F12). Reload, run a review: every
     request goes to `localhost` only — no `api.anthropic.com`, no Google.
     (DevTools shows the browser → .NET hop; the .NET → Ollama hop is local and
     server-side, which the Wi-Fi-off test covers.)

The first request is slower (the model loads into memory); pre-warm it before
the talk with one `ollama run` call so the live demo is snappy.

---

## 6. Configuration

`appsettings.json`:
```json
"Ollama": { "BaseUrl": "http://localhost:11434", "Model": "gemma4:e4b" }
```

- **Output language** is set in `BuildPrompt()` inside `CodeReviewService.cs`.
- `num_ctx` is raised to 8192 there too — the default 4K context is too small
  for real code. Very large inputs need more.
- Low `temperature` (0.2) keeps reviews consistent rather than creative.

---

## 7. Troubleshooting

| Symptom | Fix |
|---------|-----|
| "Could not reach Ollama" | Start `ollama serve`; confirm http://localhost:11434 responds. |
| "Ollama responded with 404" | Model not pulled: `ollama pull gemma4:e4b`. |
| First response very slow | Normal — model loading into VRAM. Pre-warm before the talk. |
| Out-of-memory / very slow | Switch to `gemma4:e2b` in `appsettings.json`. |
| UI loads but review fails in dev | Check the Vite proxy port matches the `dotnet run` http port. |
| Blank page after build | Confirm `npm run build` wrote into `CodeReviewAssistant/wwwroot`. |

---

## 8. Talking points

- **Local / private / zero per-request cost** — the core story.
- **The prompt is the product** — same pipeline reviews code or (with one prompt
  swap) writes commit messages; see the `CommitMessageGenerator` backup demo.
- **Architecture decision** — local vs. API vs. hybrid; pair with the cost-curve slide.
- **Clean minimal API at scale** — each concern in its own file, `Program.cs` just
  wires them; new features get their own `XyzEndpoints.cs` + `MapXyzEndpoints()`.
- **Extend it later** — caching identical reviews, batch mode, a VS Code extension,
  or fine-tuning Gemma on your team's standards (allowed under Apache 2.0).