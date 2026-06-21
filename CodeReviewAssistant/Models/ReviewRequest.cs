namespace CodeReviewAssistant.Models;

public sealed record ReviewRequest(
    string Code,
    string? Language
);