using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using SideLearning.Api.IntegrationTests.Infrastructure;

namespace SideLearning.Api.IntegrationTests.Topics;

[Collection(IntegrationTestCollection.Name)]
public sealed class TopicEndpointsTests(IntegrationTestWebAppFactory factory)
{
    [Fact]
    public async Task List_topics_returns_200_and_paging_shape()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        var response = await client.GetAsync("/api/v1/topics?page=1&pageSize=20");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        payload.RootElement.GetProperty("items").ValueKind.Should().Be(JsonValueKind.Array);
        payload.RootElement.GetProperty("page").GetInt32().Should().Be(1);
        payload.RootElement.GetProperty("pageSize").GetInt32().Should().Be(20);
        payload.RootElement.GetProperty("totalCount").GetInt32().Should().BeGreaterThanOrEqualTo(0);
    }
}
