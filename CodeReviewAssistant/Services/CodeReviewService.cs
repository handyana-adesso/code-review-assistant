using CodeReviewAssistant.Exceptions;
using CodeReviewAssistant.Models;
using Microsoft.Extensions.Options;

namespace CodeReviewAssistant.Services;

public sealed class CodeReviewService(
    HttpClient httpClient,
    IOptions<OllamaOptions> options) : ICodeReviewService
{
    private readonly OllamaOptions _options = options.Value;

    public async Task<ReviewResult> ReviewAsync(
        ReviewRequest request, 
        CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            model = _options.Model,
            prompt = BuildPrompt(request),
            stream = false,
            options = new
            {
                temperature = 0.2,  // low -> consistent, focused reviews
                num_ctx = 8192      // IMPORTANT: Ollama defaults to a small context; raise it for real code
            }
        };

        using var response = await httpClient
            .PostAsJsonAsync("/api/generate", payload, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new OllamaException(
                $"Ollama responded with {(int)response.StatusCode}. " +
                $"Is 'ollama serve' running and the model pulled ('ollama pull {_options.Model}')?");
        }

        var result = await response.Content.ReadFromJsonAsync<OllamaGenerateResponse>(cancellationToken);

        return new(
            Model: _options.Model,
            Review: result?.Response?.Trim() ?? "(empty response)",
            DurationMs: result?.TotalDuration is { } ns ? ns / 1_000_000 : null);
    }

    private static string BuildPrompt(ReviewRequest request)
    {
        var language = request.Language ?? "";
        return "You are an experienced senior software engineer performing a code review.\n" +
            "Analyze the following code and answer ONLY in English, structured into exactly these sections:\n" +
            "1. Security\n2. Performance\n3. Best Practices / Readability\n4. Concrete suggestion (short code snippet)\n\n" +
            "Be precise and only flag real issues. If a section has nothing, write 'No issues found'.\n\n" +
            "```" + language + "\n" +
            request.Code + "\n```";
    }
}