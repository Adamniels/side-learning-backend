using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using SideLearning.Api.IntegrationTests.Infrastructure;

namespace SideLearning.Api.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public sealed class HealthTests(IntegrationTestWebAppFactory factory)
{
    [Fact]
    public async Task Health_endpoint_returns_ok()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });

        var response = await client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
