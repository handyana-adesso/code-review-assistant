import { useState } from "react";

type Review = { model: string; review: string; durationMs: number | null };
type ApiError = { error: string };

const LANGUAGES = ["csharp", "typescript", "javascript", "python", "java", "sql"];

const SAMPLE = `public string GetUser(string id)
{
    var conn = new SqlConnection(connStr);
    conn.Open();
    var cmd = new SqlCommand("SELECT * FROM Users WHERE Id = '" + id + "'", conn);
    var reader = cmd.ExecuteReader();
    reader.Read();
    return reader["Name"].ToString();
}`;

export default function App() {
  const [code, setCode] = useState("");
  const [language, setLanguage] = useState("csharp");
  const [review, setReview] = useState<Review | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  async function runReview() {
    if (!code.trim()) return;
    setLoading(true);
    setReview(null);
    setError(null);

    try {
      const res = await fetch("/api/review", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ code, language }),
      });
      const data: Review | ApiError = await res.json();

      if (!res.ok || "error" in data) {
        setError("error" in data ? data.error : "Unknown error");
        return;
      }
      setReview(data);
    } catch {
      setError("Network error. Is the .NET API running (dotnet run)?");
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="app">
      <header>
        <div className="logo">🤖</div>
        <div>
          <h1>Code Review Assistant</h1>
        </div>
        <span className="badge">Model: gemma4:e4b</span>
      </header>

      <main className="wrap">
        <section className="panel">
          <h2>Your code</h2>
          <div className="row">
            <label htmlFor="lang">Language</label>
            <select id="lang" value={language} onChange={(e) => setLanguage(e.target.value)}>
              {LANGUAGES.map((l) => <option key={l} value={l}>{l}</option>)}
            </select>
            <button className="ghost" onClick={() => { setLanguage("csharp"); setCode(SAMPLE); }}>
              Load example
            </button>
          </div>
          <textarea
            value={code}
            onChange={(e) => setCode(e.target.value)}
            placeholder="Paste your code and click Run review…"
          />
          <button className="primary" onClick={runReview} disabled={loading}>
            {loading ? "Reviewing…" : "Run review"}
          </button>
        </section>

        <section className="panel">
          <h2>Gemma says</h2>
          <div className="result">
            {loading && <div className="placeholder"><div className="spinner" />Analyzing locally…</div>}
            {error && <span className="err">⚠ {error}</span>}
            {review && <pre>{review.review}</pre>}
            {!loading && !error && !review && (
              <div className="placeholder">The review appears here — generated locally by Gemma 4.</div>
            )}
          </div>
          {review && (
            <div className="meta">
              Model: {review.model}
              {review.durationMs != null && ` · ${(review.durationMs / 1000).toFixed(1)} s`}
            </div>
          )}
        </section>
      </main>

      <footer>🔒 Everything stays local. Your code only goes to Ollama on your machine — no cloud, no API key.</footer>
    </div>
  );
}