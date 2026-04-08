using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SideLearning.Application.Features.Sessions.Common;
using SideLearning.Application.Features.Sessions.List;

namespace SideLearning.Api.Features.Sessions;

public static class SessionEndpoints
{
    public static void MapSessionEndpoints(this RouteGroupBuilder group)
    {
        var users = group.MapGroup("/users")
            .WithTags("Sessions")
            .RequireAuthorization(new AuthorizeAttribute
            {
                AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme
            });
            
        var sessions = users.MapGroup("/me/sessions");

        sessions.MapGet("", async Task<IResult> (
                HttpContext httpContext,
                ListSessionsQueryHandler handler,
                CancellationToken cancellationToken) =>
            {
                var userId = GetCurrentUserId(httpContext.User);
                var result = await handler.HandleAsync(userId, cancellationToken);
                return Results.Ok(result);
            })
            .Produces<IReadOnlyCollection<SessionDto>>()
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
}
