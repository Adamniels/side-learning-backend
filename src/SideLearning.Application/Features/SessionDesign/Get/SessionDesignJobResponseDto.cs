using System.Text.Json.Serialization;

namespace SideLearning.Application.Features.SessionDesign.Get;

public sealed class SessionDesignJobResponseDto
{
    [JsonPropertyName("jobId")]
    public Guid JobId { get; init; }

    [JsonPropertyName("status")]
    public string Status { get; init; } = "";

    [JsonPropertyName("createdAtUtc")]
    public DateTimeOffset CreatedAtUtc { get; init; }

    [JsonPropertyName("startedAtUtc")]
    public DateTimeOffset? StartedAtUtc { get; init; }

    [JsonPropertyName("completedAtUtc")]
    public DateTimeOffset? CompletedAtUtc { get; init; }

    [JsonPropertyName("createdSessionId")]
    public Guid? CreatedSessionId { get; init; }

    /// <summary>Full designer result JSON when <see cref="Status"/> is <c>succeeded</c>.</summary>
    [JsonPropertyName("sessionDesignResult")]
    public object? SessionDesignResult { get; init; }

    [JsonPropertyName("error")]
    public SessionDesignJobErrorDto? Error { get; init; }
}

public sealed class SessionDesignJobErrorDto
{
    [JsonPropertyName("code")]
    public string? Code { get; init; }

    [JsonPropertyName("message")]
    public string Message { get; init; } = "";
}
