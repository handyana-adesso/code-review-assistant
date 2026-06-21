using CodeReviewAssistant.Models;

namespace CodeReviewAssistant.Services;

public interface ICodeReviewService
{
    Task<ReviewResult> ReviewAsync(ReviewRequest request, CancellationToken cancellationToken = default);
}