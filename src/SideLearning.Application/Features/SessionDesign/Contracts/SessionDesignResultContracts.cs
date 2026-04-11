using System.Text.Json;
using System.Text.Json.Serialization;

namespace SideLearning.Application.Features.SessionDesign.Contracts;

/// <summary>
/// JSON shape aligned with Python <c>SessionDesignResult</c> (subset used for persistence mapping).
/// </summary>
public sealed class SessionDesignResultDto
{
    [JsonPropertyName("session_payload")]
    public SessionPayloadDto SessionPayload { get; init; } = null!;

    [JsonPropertyName("designer_metadata")]
    public JsonElement? DesignerMetadata { get; init; }

    [JsonPropertyName("suggested_resources")]
    public List<JsonElement> SuggestedResources { get; init; } = [];
}

public sealed class SessionPayloadDto
{
    [JsonPropertyName("title")]
    public string Title { get; init; } = "";

    [JsonPropertyName("summary")]
    public string Summary { get; init; } = "";

    [JsonPropertyName("difficulty_alignment")]
    public string DifficultyAlignment { get; init; } = "";

    [JsonPropertyName("goal")]
    public string Goal { get; init; } = "";

    [JsonPropertyName("context")]
    public string Context { get; init; } = "";

    [JsonPropertyName("hands_on")]
    public string HandsOn { get; init; } = "";

    [JsonPropertyName("hands_on_expected_output")]
    public string HandsOnExpectedOutput { get; init; } = "";

    [JsonPropertyName("extension")]
    public string Extension { get; init; } = "";

    [JsonPropertyName("subject_areas")]
    public List<string> SubjectAreas { get; init; } = [];

    [JsonPropertyName("estimated_duration_in_minutes")]
    public int? EstimatedDurationInMinutes { get; init; }
}
