using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SideLearning.Application.Features.Users.Interests.Add;
using SideLearning.Application.Features.Users.Interests.Common;
using SideLearning.Application.Features.Users.Interests.List;
using SideLearning.Application.Features.Users.Interests.Remove;
using SideLearning.Application.Features.Users.Interests.Update;

namespace SideLearning.Api.Features.Users;

public static class UserInterestEndpoints
{
    public static void MapUserInterestEndpoints(this RouteGroupBuilder group)
    {
        var users = group.MapGroup("/users")
            .WithTags("Users")
            .RequireAuthorization(new AuthorizeAttribute
            {
                AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme
            });
        var interests = users.MapGroup("/me/interests");

        interests.MapGet("", async Task<IResult> (
                HttpContext httpContext,
                ListUserInterestsQueryHandler handler,
                CancellationToken cancellationToken) =>
            {
                var userId = GetCurrentUserId(httpContext.User);
                var result = await handler.HandleAsync(userId, cancellationToken);
                return Results.Ok(result);
            })
            .Produces<IReadOnlyCollection<UserInterestDto>>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        interests.MapPost("", async Task<IResult> (
                HttpContext httpContext,
                [FromBody] UpsertUserInterestRequest request,
                AddUserInterestCommandHandler handler,
                CancellationToken cancellationToken) =>
            {
                var userId = GetCurrentUserId(httpContext.User);
                var result = await handler.HandleAsync(
                    new AddUserInterestCommand(userId, request.Label, request.Weight, request.Context),
                    cancellationToken);
                return Results.Created($"/api/v1/users/me/interests/{Uri.EscapeDataString(result.Label)}", result);
            })
            .Produces<UserInterestDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        interests.MapPut("/{label}", async Task<IResult> (
                HttpContext httpContext,
                [FromRoute] string label,
                [FromBody] UpsertUserInterestRequest request,
                UpdateUserInterestCommandHandler handler,
                CancellationToken cancellationToken) =>
            {
                var userId = GetCurrentUserId(httpContext.User);
                var result = await handler.HandleAsync(
                    new UpdateUserInterestCommand(userId, label, request.Label, request.Weight, request.Context),
                    cancellationToken);
                return Results.Ok(result);
            })
            .Produces<UserInterestDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        interests.MapDelete("/{label}", async Task<IResult> (
                HttpContext httpContext,
                [FromRoute] string label,
                RemoveUserInterestCommandHandler handler,
                CancellationToken cancellationToken) =>
            {
                var userId = GetCurrentUserId(httpContext.User);
                await handler.HandleAsync(new RemoveUserInterestCommand(userId, label), cancellationToken);
                return Results.NoContent();
            })
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
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

    public sealed record UpsertUserInterestRequest(string Label, float Weight, string? Context);
}
