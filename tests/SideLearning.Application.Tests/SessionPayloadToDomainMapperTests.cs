using SideLearning.Application.Features.SessionDesign;
using SideLearning.Application.Features.SessionDesign.Contracts;
using SideLearning.Domain.Sessions;

namespace SideLearning.Application.Tests;

public sealed class SessionPayloadToDomainMapperTests
{
    [Fact]
    public void CreateSessionFromPayload_maps_context_and_hands_on()
    {
        var userId = Guid.NewGuid();
        var payload = new SessionPayloadDto
        {
            Title = "Test title",
            Summary = "Test summary",
            DifficultyAlignment = "Matches beginner level.",
            Goal = "Learn the thing",
            Context = "Background context text.",
            HandsOn = "Do the exercise",
            HandsOnExpectedOutput = "You see output",
            Extension = "Go further",
            SubjectAreas = ["TopicA", "TopicB"],
            EstimatedDurationInMinutes = 45
        };

        var session = SessionPayloadToDomainMapper.CreateSessionFromPayload(userId, payload);

        Assert.Equal(userId, session.UserId);
        Assert.Equal("Test title", session.Title);
        Assert.Equal(SessionStatus.Draft, session.Status);
        Assert.Equal("Background context text.", session.Context.Explanation);
        Assert.Equal("Matches beginner level.", session.Context.WhyItMatters);
        Assert.Equal("Do the exercise", session.HandsOn.Instructions);
        Assert.Equal("You see output", session.HandsOn.ExpectedOutput);
        Assert.Equal(new[] { "TopicA", "TopicB" }, session.SubjectAreas.Select(a => a.Value).ToArray());
        Assert.Equal(45, session.EstimatedDurationInMinutes);
    }

    [Fact]
    public void CreateSessionFromPayload_uses_default_why_when_difficulty_empty()
    {
        var userId = Guid.NewGuid();
        var payload = new SessionPayloadDto
        {
            Title = "T",
            Summary = "S",
            DifficultyAlignment = "   ",
            Goal = "G",
            Context = "C",
            HandsOn = "H",
            HandsOnExpectedOutput = "O",
            Extension = "E",
            SubjectAreas = [],
            EstimatedDurationInMinutes = 60
        };

        var session = SessionPayloadToDomainMapper.CreateSessionFromPayload(userId, payload);

        Assert.Equal("Aligned with your learning goals.", session.Context.WhyItMatters);
    }
}
