using System.Text.Json.Serialization;

namespace SideLearning.Application.Features.SessionDesign.Contracts;

public sealed class UserLearningContextDto
{
    [JsonPropertyName("user_id")]
    public Guid UserId { get; init; }

    [JsonPropertyName("interests")]
    public List<UserInterestItemDto> Interests { get; init; } = [];

    [JsonPropertyName("completed_sessions")]
    public List<PastSessionItemDto> CompletedSessions { get; init; } = [];

    [JsonPropertyName("uncompleted_sessions")]
    public List<PastSessionItemDto> UncompletedSessions { get; init; } = [];
}

public sealed class UserInterestItemDto
{
    [JsonPropertyName("label")]
    public string Label { get; init; } = "";

    [JsonPropertyName("weight")]
    public double Weight { get; init; }

    [JsonPropertyName("context")]
    public string Context { get; init; } = "";
}

public sealed class PastSessionItemDto
{
    [JsonPropertyName("title")]
    public string Title { get; init; } = "";

    [JsonPropertyName("summary")]
    public string Summary { get; init; } = "";

    [JsonPropertyName("topics")]
    public List<string> Topics { get; init; } = [];

    [JsonPropertyName("completed_at")]
    public string? CompletedAt { get; init; }

    [JsonPropertyName("last_touched_at")]
    public string? LastTouchedAt { get; init; }
}
