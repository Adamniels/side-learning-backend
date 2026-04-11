using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SideLearning.Application.Configuration;
using SideLearning.Application.Features.SessionDesign.Callback;

namespace SideLearning.Api.Features.SessionDesign;

public static class InternalSessionDesignCallbackEndpoints
{
    public const string SecretHeaderName = "X-Session-Designer-Secret";

    public static void MapInternalSessionDesignCallbackEndpoints(this WebApplication app)
    {
        app.MapPost(
                "/internal/session-design/jobs/{jobId:guid}/callback",
                async Task<IResult> (
                    Guid jobId,
                    HttpRequest request,
                    IOptions<SessionDesignerOptions> designerOptions,
                    ProcessSessionDesignCallbackCommandHandler handler,
                    CancellationToken cancellationToken) =>
                {
                    if (!request.Headers.TryGetValue(SecretHeaderName, out var providedSecret))
                    {
                        return Results.Unauthorized();
                    }

                    var expected = designerOptions.Value.SharedSecret;
                    if (string.IsNullOrEmpty(expected) ||
                        !string.Equals(providedSecret.ToString(), expected, StringComparison.Ordinal))
                    {
                        return Results.Unauthorized();
                    }

                    request.EnableBuffering();
                    using var reader = new StreamReader(request.Body, leaveOpen: true);
                    var rawBody = await reader.ReadToEndAsync(cancellationToken);
                    request.Body.Position = 0;

                    await handler.HandleAsync(new ProcessSessionDesignCallbackCommand(jobId, rawBody), cancellationToken);
                    return Results.Ok();
                })
            .AllowAnonymous()
            .ExcludeFromDescription()
            .WithTags("Internal");
    }
}
