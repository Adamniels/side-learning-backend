using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using SideLearning.Api.IntegrationTests.Infrastructure;

namespace SideLearning.Api.IntegrationTests.Auth;

[Collection(IntegrationTestCollection.Name)]
public sealed class AuthEndpointsTests(IntegrationTestWebAppFactory factory)
{
    [Fact]
    public async Task Register_returns_201_for_new_user()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });

        var email = $"user_{Guid.NewGuid():N}@example.com";
        var response = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email,
            password = "Password1!",
            displayName = "Integration User"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        payload.RootElement.GetProperty("userId").GetGuid().Should().NotBeEmpty();
        payload.RootElement.GetProperty("accessToken").GetString().Should().NotBeNullOrWhiteSpace();
        payload.RootElement.GetProperty("refreshToken").GetString().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Register_returns_409_for_duplicate_email()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });

        var email = $"dup_{Guid.NewGuid():N}@example.com";
        var body = new { email, password = "Password1!", displayName = "Dup User" };

        var first = await client.PostAsJsonAsync("/api/v1/auth/register", body);
        first.StatusCode.Should().Be(HttpStatusCode.Created);

        var second = await client.PostAsJsonAsync("/api/v1/auth/register", body);
        second.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var payload = JsonDocument.Parse(await second.Content.ReadAsStringAsync());
        payload.RootElement.GetProperty("code").GetString().Should().Be("email_already_exists");
    }

    [Fact]
    public async Task Login_returns_200_with_tokens()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });

        var email = $"login_{Guid.NewGuid():N}@example.com";
        await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email,
            password = "Password1!",
            displayName = "Login User"
        });

        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email,
            password = "Password1!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        payload.RootElement.GetProperty("accessToken").GetString().Should().NotBeNullOrWhiteSpace();
        payload.RootElement.GetProperty("refreshToken").GetString().Should().NotBeNullOrWhiteSpace();
        payload.RootElement.GetProperty("userId").ValueKind.Should().Be(JsonValueKind.Null);
    }

    [Fact]
    public async Task Login_returns_401_for_invalid_password()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });

        var email = $"invalid_login_{Guid.NewGuid():N}@example.com";
        await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email,
            password = "Password1!",
            displayName = "Invalid Login User"
        });

        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email,
            password = "WrongPassword1!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        payload.RootElement.GetProperty("code").GetString().Should().Be("invalid_credentials");
    }

    [Fact]
    public async Task Refresh_rotates_token_and_revokes_previous()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });

        var email = $"refresh_{Guid.NewGuid():N}@example.com";
        var register = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email,
            password = "Password1!",
            displayName = "Refresh User"
        });

        var registerPayload = JsonDocument.Parse(await register.Content.ReadAsStringAsync());
        var initialRefreshToken = registerPayload.RootElement.GetProperty("refreshToken").GetString();
        initialRefreshToken.Should().NotBeNullOrWhiteSpace();

        var refresh = await client.PostAsJsonAsync("/api/v1/auth/refresh", new { refreshToken = initialRefreshToken });
        refresh.StatusCode.Should().Be(HttpStatusCode.OK);

        var refreshPayload = JsonDocument.Parse(await refresh.Content.ReadAsStringAsync());
        var rotatedRefreshToken = refreshPayload.RootElement.GetProperty("refreshToken").GetString();
        rotatedRefreshToken.Should().NotBeNullOrWhiteSpace();
        rotatedRefreshToken.Should().NotBe(initialRefreshToken);
        refreshPayload.RootElement.GetProperty("userId").ValueKind.Should().Be(JsonValueKind.Null);

        var secondUse = await client.PostAsJsonAsync("/api/v1/auth/refresh", new { refreshToken = initialRefreshToken });
        secondUse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_returns_401_for_invalid_token()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });

        var response = await client.PostAsJsonAsync("/api/v1/auth/refresh", new { refreshToken = "invalid-token" });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        payload.RootElement.GetProperty("code").GetString().Should().Be("invalid_refresh_token");
    }

    [Fact]
    public async Task Revoke_then_refresh_returns_401()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });

        var email = $"revoke_{Guid.NewGuid():N}@example.com";
        var register = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email,
            password = "Password1!",
            displayName = "Revoke User"
        });

        var registerPayload = JsonDocument.Parse(await register.Content.ReadAsStringAsync());
        var refreshToken = registerPayload.RootElement.GetProperty("refreshToken").GetString();
        refreshToken.Should().NotBeNullOrWhiteSpace();

        var revoke = await client.PostAsJsonAsync("/api/v1/auth/revoke", new { refreshToken });
        revoke.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var refreshed = await client.PostAsJsonAsync("/api/v1/auth/refresh", new { refreshToken });
        refreshed.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Revoke_is_idempotent_for_unknown_token()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });

        var response = await client.PostAsJsonAsync("/api/v1/auth/revoke", new { refreshToken = "unknown-token" });
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
