using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using SideLearning.Api.IntegrationTests.Infrastructure;
using SideLearning.Application.Abstractions.SessionDesign;

namespace SideLearning.Api.IntegrationTests.SessionDesign;

[Collection(IntegrationTestCollection.Name)]
public sealed class SessionDesignJobEndpointsTests(IntegrationTestWebAppFactory factory)
{
    /// <summary>Declaration order: tests that claim the next queued job must run before <see cref="Post_job_returns_202_and_get_shows_queued_when_worker_disabled"/>.</summary>
    [Fact]
    public async Task Callback_without_secret_returns_401()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });

        var response = await client.PostAsJsonAsync(
            $"/internal/session-design/jobs/{Guid.NewGuid()}/callback",
            new { outcome = "failed", error = new { message = "x" } });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Callback_after_claim_creates_session_and_marks_job_succeeded()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        var (token, _) = await RegisterAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var post = await client.PostAsync("/api/v1/users/me/session-design/jobs", null);
        post.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var jobId = JsonDocument.Parse(await post.Content.ReadAsStringAsync()).RootElement.GetProperty("jobId").GetGuid();

        bool claimed;
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<ISessionDesignJobRepository>();
            claimed = await repo.TryClaimJobByIdAsync(jobId, CancellationToken.None);
        }

        claimed.Should().BeTrue();

        var callbackBody = new
        {
            outcome = "succeeded",
            sessionDesignResult = new
            {
                session_payload = new
                {
                    title = "Designed session",
                    summary = "Summary line",
                    difficulty_alignment = "Beginner",
                    goal = "Goal text",
                    context = "Context body",
                    hands_on = "Hands on steps",
                    hands_on_expected_output = "Expected",
                    extension = "Extension text",
                    subject_areas = new[] { "Area1" },
                    estimated_duration_in_minutes = 45
                },
                designer_metadata = new { },
                suggested_resources = Array.Empty<object>()
            }
        };

        using var callbackRequest = new HttpRequestMessage(
            HttpMethod.Post,
            $"/internal/session-design/jobs/{jobId}/callback");
        callbackRequest.Headers.Add("X-Session-Designer-Secret", "test_session_designer_secret");
        callbackRequest.Content = JsonContent.Create(callbackBody);
        var callbackResponse = await client.SendAsync(callbackRequest);
        callbackResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var get = await client.GetAsync($"/api/v1/users/me/session-design/jobs/{jobId}");
        get.StatusCode.Should().Be(HttpStatusCode.OK);
        var getDoc = JsonDocument.Parse(await get.Content.ReadAsStringAsync());
        getDoc.RootElement.GetProperty("status").GetString().Should().Be("succeeded");
        getDoc.RootElement.GetProperty("createdSessionId").GetGuid().Should().NotBeEmpty();

        using var callbackRequest2 = new HttpRequestMessage(
            HttpMethod.Post,
            $"/internal/session-design/jobs/{jobId}/callback");
        callbackRequest2.Headers.Add("X-Session-Designer-Secret", "test_session_designer_secret");
        callbackRequest2.Content = JsonContent.Create(callbackBody);
        var callback2 = await client.SendAsync(callbackRequest2);
        callback2.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Post_job_returns_202_and_get_shows_queued_when_worker_disabled()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        var (token, _) = await RegisterAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var post = await client.PostAsync("/api/v1/users/me/session-design/jobs", null);
        post.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var postDoc = JsonDocument.Parse(await post.Content.ReadAsStringAsync());
        var jobId = postDoc.RootElement.GetProperty("jobId").GetGuid();

        var get = await client.GetAsync($"/api/v1/users/me/session-design/jobs/{jobId}");
        get.StatusCode.Should().Be(HttpStatusCode.OK);
        var getDoc = JsonDocument.Parse(await get.Content.ReadAsStringAsync());
        getDoc.RootElement.GetProperty("status").GetString().Should().Be("queued");
    }

    private static async Task<(string Token, Guid UserId)> RegisterAsync(HttpClient client)
    {
        var email = $"sd_{Guid.NewGuid():N}@example.com";
        const string password = "Password1!";
        var register = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email,
            password,
            displayName = "SD User"
        });
        register.StatusCode.Should().Be(HttpStatusCode.Created);
        var regDoc = JsonDocument.Parse(await register.Content.ReadAsStringAsync());
        var userId = regDoc.RootElement.GetProperty("userId").GetGuid();

        var login = await client.PostAsJsonAsync("/api/v1/auth/login", new { email, password });
        login.StatusCode.Should().Be(HttpStatusCode.OK);
        var loginDoc = JsonDocument.Parse(await login.Content.ReadAsStringAsync());
        var token = loginDoc.RootElement.GetProperty("accessToken").GetString()!;
        return (token, userId);
    }
}
