using CodeReviewAssistant.Endpoints;
using CodeReviewAssistant.Models;
using CodeReviewAssistant.Services;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<OllamaOptions>(
    builder.Configuration.GetSection(OllamaOptions.SectionName));

builder.Services.AddHttpClient<ICodeReviewService, CodeReviewService>((sp, client) =>
{
    var options = sp.GetRequiredService<IOptions<OllamaOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl);
    client.Timeout = TimeSpan.FromMinutes(5); // first call loads the model into memory
});

builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseDefaultFiles();   // serve wwwroot/index.html at "/"
app.UseStaticFiles();

app.MapHealthChecks("/health");
app.MapReviewEndpoints();

app.Run();
