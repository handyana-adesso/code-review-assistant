namespace CodeReviewAssistant.Models;

public sealed record ReviewResult(
    string Model,
    string Review,
    long? DurationMs
);