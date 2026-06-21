namespace CodeReviewAssistant.Exceptions;

public sealed class OllamaException(string message)
    : Exception(message);