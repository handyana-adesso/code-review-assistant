using System.Text.Json.Serialization;

namespace CodeReviewAssistant.Models;

public sealed class OllamaGenerateResponse
{
    [JsonPropertyName("response")]
    public string? Response { get; set; }

    [JsonPropertyName("total_duration")]
    public long? TotalDuration { get; set; } // in nanoseconds
}