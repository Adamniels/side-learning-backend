using Microsoft.AspNetCore.Mvc;
using SideLearning.Application.Abstractions.Authentication;
using SideLearning.Application.Features.Auth.Login;
using SideLearning.Application.Features.Auth.Refresh;
using SideLearning.Application.Features.Auth.Register;
using SideLearning.Application.Features.Auth.Revoke;

namespace SideLearning.Api.Features.Auth;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this RouteGroupBuilder group)
    {
        var auth = group.MapGroup("/auth").WithTags("Auth");

        auth.MapPost("/register", async Task<IResult> (
                [FromBody] RegisterRequest request,
                RegisterCommandHandler handler,
                CancellationToken cancellationToken) =>
            {
                var cmd = new RegisterCommand(request.Email, request.Password, request.DisplayName);
                var result = await handler.HandleAsync(cmd, cancellationToken);
                return Results.Created($"/api/v1/users/{result.UserId}", new AuthResponse(
                    result.UserId,
                    result.Tokens.AccessToken,
                    result.Tokens.RefreshToken,
                    result.Tokens.AccessTokenExpiresAtUtc,
                    result.Tokens.RefreshTokenExpiresAtUtc));
            })
            .AllowAnonymous()
            .Produces(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);

        auth.MapPost("/login", async Task<IResult> (
                [FromBody] LoginRequest request,
                LoginCommandHandler handler,
                CancellationToken cancellationToken) =>
            {
                var tokens = await handler.HandleAsync(new LoginCommand(request.Email, request.Password), cancellationToken);
                return Results.Ok(Map(tokens));
            })
            .AllowAnonymous()
            .Produces<AuthResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        auth.MapPost("/refresh", async Task<IResult> (
                [FromBody] RefreshRequest request,
                RefreshTokenCommandHandler handler,
                CancellationToken cancellationToken) =>
            {
                var tokens = await handler.HandleAsync(new RefreshTokenCommand(request.RefreshToken), cancellationToken);
                return Results.Ok(Map(tokens));
            })
            .AllowAnonymous()
            .Produces<AuthResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        auth.MapPost("/revoke", async Task<IResult> (
                [FromBody] RevokeRequest request,
                RevokeRefreshCommandHandler handler,
                CancellationToken cancellationToken) =>
            {
                await handler.HandleAsync(new RevokeRefreshCommand(request.RefreshToken), cancellationToken);
                return Results.NoContent();
            })
            .AllowAnonymous()
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest);
    }

    private static AuthResponse Map(AuthTokenPair tokens) => new(
        null,
        tokens.AccessToken,
        tokens.RefreshToken,
        tokens.AccessTokenExpiresAtUtc,
        tokens.RefreshTokenExpiresAtUtc);

    public sealed record RegisterRequest(string Email, string Password, string? DisplayName);

    public sealed record LoginRequest(string Email, string Password);

    public sealed record RefreshRequest(string RefreshToken);

    public sealed record RevokeRequest(string RefreshToken);

    public sealed record AuthResponse(
        Guid? UserId,
        string AccessToken,
        string RefreshToken,
        DateTimeOffset AccessTokenExpiresAtUtc,
        DateTimeOffset RefreshTokenExpiresAtUtc);
}
