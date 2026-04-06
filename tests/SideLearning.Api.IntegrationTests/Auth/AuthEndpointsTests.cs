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
    }
}
