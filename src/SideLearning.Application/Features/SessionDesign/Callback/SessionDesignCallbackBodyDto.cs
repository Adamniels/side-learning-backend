using System.Text.Json.Serialization;
using SideLearning.Application.Features.SessionDesign.Contracts;

namespace SideLearning.Application.Features.SessionDesign.Callback;

public sealed class SessionDesignCallbackBodyDto
{
    [JsonPropertyName("outcome")]
    public string Outcome { get; init; } = "";

    /// <summary>Designer output; property name matches agent JSON (camelCase).</summary>
    [JsonPropertyName("sessionDesignResult")]
    public SessionDesignResultDto? SessionDesignResult { get; init; }

    [JsonPropertyName("error")]
    public SessionDesignCallbackErrorDto? Error { get; init; }
}

public sealed class SessionDesignCallbackErrorDto
{
    [JsonPropertyName("message")]
    public string Message { get; init; } = "";

    [JsonPropertyName("code")]
    public string? Code { get; init; }
}
