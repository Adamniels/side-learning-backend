using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SideLearning.Application.Features.SessionDesign.Enqueue;
using SideLearning.Application.Features.SessionDesign.Get;

namespace SideLearning.Api.Features.SessionDesign;

public static class SessionDesignEndpoints
{
    public static void MapSessionDesignEndpoints(this RouteGroupBuilder group)
    {
        var users = group.MapGroup("/users")
            .WithTags("SessionDesign")
            .RequireAuthorization(new AuthorizeAttribute
            {
                AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme
            });

        var jobs = users.MapGroup("/me/session-design/jobs");

        jobs.MapPost("", async Task<IResult> (
                HttpContext httpContext,
                EnqueueSessionDesignJobCommandHandler handler,
                CancellationToken cancellationToken) =>
            {
                var userId = GetCurrentUserId(httpContext.User);
                var jobId = await handler.HandleAsync(new EnqueueSessionDesignJobCommand(userId), cancellationToken);
                return Results.Accepted(
                    $"/api/v1/users/me/session-design/jobs/{jobId}",
                    new EnqueueJobResponse(jobId, $"/api/v1/users/me/session-design/jobs/{jobId}"));
            })
            .Produces<EnqueueJobResponse>(StatusCodes.Status202Accepted)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        jobs.MapGet("{jobId:guid}", async Task<IResult> (
                HttpContext httpContext,
                Guid jobId,
                GetSessionDesignJobQueryHandler handler,
                CancellationToken cancellationToken) =>
            {
                var userId = GetCurrentUserId(httpContext.User);
                var dto = await handler.HandleAsync(userId, jobId, cancellationToken);
                return Results.Ok(dto);
            })
            .Produces<SessionDesignJobResponseDto>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static Guid GetCurrentUserId(ClaimsPrincipal principal)
    {
        var raw = principal.FindFirstValue(ClaimTypes.NameIdentifier) ??
                  principal.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
                  principal.FindFirst("sub")?.Value;

        return Guid.TryParse(raw, out var userId)
            ? userId
            : throw new UnauthorizedAccessException("Authenticated user id claim was missing or invalid.");
    }

    public sealed record EnqueueJobResponse(Guid JobId, string StatusUrl);
}
