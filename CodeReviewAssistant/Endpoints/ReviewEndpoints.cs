using CodeReviewAssistant.Exceptions;
using CodeReviewAssistant.Models;
using CodeReviewAssistant.Services;
using Microsoft.AspNetCore.Mvc;

namespace CodeReviewAssistant.Endpoints;

public static class ReviewEndpoints
{
    public static IEndpointRouteBuilder MapReviewEndpoints(
        this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/review")
            .WithTags("Review");

        group.MapPost("/", HandleReviewAsync)
            .WithName("HandleReview")
            .WithSummary("")
            .WithDescription("")
            .Produces<ReviewResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status502BadGateway);

        return app;
    }

    private static async Task<IResult> HandleReviewAsync(
        [FromServices] ICodeReviewService codeReviewService,
        [FromBody] ReviewRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
        {
            return Results.BadRequest(new
            {
                error = "Please paste some code."
            });
        }

        try
        {
            var result = await codeReviewService
                .ReviewAsync(request, cancellationToken);
            return Results.Json(result);
        }
        catch (OllamaException ex)
        {
            return Results.Json(new
            {
                error = ex.Message,
                statusCode = StatusCodes.Status502BadGateway
            });
        }
        catch (HttpRequestException)
        {
            return Results.Json(new
            {
                error = "Could not reach Ollama. Start it with 'ollama serve' " +
                        "and check it is reachable at the configured base URL."
            }, statusCode: 502);
        }
    }
}
