using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using SideLearning.Api.IntegrationTests.Infrastructure;

namespace SideLearning.Api.IntegrationTests.Users;

[Collection(IntegrationTestCollection.Name)]
public sealed class UserInterestEndpointsTests(IntegrationTestWebAppFactory factory)
{
    [Fact]
    public async Task Interests_endpoint_returns_401_when_missing_token()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });

        var response = await client.GetAsync("/api/v1/users/me/interests");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Interests_endpoint_returns_401_for_invalid_token()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "not-a-valid-jwt");

        var response = await client.GetAsync("/api/v1/users/me/interests");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Interest_crud_roundtrip_persists_context()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        var token = await RegisterAndGetAccessTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createResponse = await client.PostAsJsonAsync("/api/v1/users/me/interests", new
        {
            label = "Cybersecurity",
            weight = 0.9f,
            context = "Improve secure token handling and auth hardening."
        });
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var listAfterCreate = await client.GetAsync("/api/v1/users/me/interests");
        listAfterCreate.StatusCode.Should().Be(HttpStatusCode.OK);
        var createdPayload = JsonDocument.Parse(await listAfterCreate.Content.ReadAsStringAsync());
        var createdInterest = Assert.Single(createdPayload.RootElement.EnumerateArray());
        createdInterest.GetProperty("label").GetString().Should().Be("Cybersecurity");
        createdInterest.GetProperty("context").GetString().Should().Be("Improve secure token handling and auth hardening.");

        var updateResponse = await client.PutAsJsonAsync("/api/v1/users/me/interests/Cybersecurity", new
        {
            label = "API Security",
            weight = 0.75f,
            context = "Concrete focus on JWT verification, refresh rotation, and replay prevention."
        });
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var listAfterUpdate = await client.GetAsync("/api/v1/users/me/interests");
        listAfterUpdate.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedPayload = JsonDocument.Parse(await listAfterUpdate.Content.ReadAsStringAsync());
        var updatedInterest = Assert.Single(updatedPayload.RootElement.EnumerateArray());
        updatedInterest.GetProperty("label").GetString().Should().Be("API Security");
        updatedInterest.GetProperty("context").GetString().Should().Be("Concrete focus on JWT verification, refresh rotation, and replay prevention.");

        var deleteResponse = await client.DeleteAsync("/api/v1/users/me/interests/API%20Security");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var listAfterDelete = await client.GetAsync("/api/v1/users/me/interests");
        listAfterDelete.StatusCode.Should().Be(HttpStatusCode.OK);
        var finalPayload = JsonDocument.Parse(await listAfterDelete.Content.ReadAsStringAsync());
        finalPayload.RootElement.GetArrayLength().Should().Be(0);
    }

    private static async Task<string> RegisterAndGetAccessTokenAsync(HttpClient client)
    {
        var email = $"interest_{Guid.NewGuid():N}@example.com";
        const string password = "Password1!";
        var register = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email,
            password,
            displayName = "Interest User"
        });

        register.StatusCode.Should().Be(HttpStatusCode.Created);

        var login = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email,
            password
        });
        login.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginPayload = JsonDocument.Parse(await login.Content.ReadAsStringAsync());
        return loginPayload.RootElement.GetProperty("accessToken").GetString()!;
    }
}
