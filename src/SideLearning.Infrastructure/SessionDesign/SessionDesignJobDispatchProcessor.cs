using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SideLearning.Application.Abstractions.SessionDesign;
using SideLearning.Application.Configuration;
using SideLearning.Application.Features.SessionDesign;
using SideLearning.Application.Features.SessionDesign.Contracts;
using SideLearning.Domain.SessionDesign;

namespace SideLearning.Infrastructure.SessionDesign;

public sealed class SessionDesignJobDispatchProcessor(
    IHttpClientFactory httpClientFactory,
    ISessionDesignJobRepository sessionDesignJobRepository,
    IUserLearningContextFactory userLearningContextFactory,
    IOptions<SessionDesignerOptions> designerOptions,
    IOptions<PublicApiCallbacksOptions> publicApiOptions,
    ILogger<SessionDesignJobDispatchProcessor> logger) : ISessionDesignJobDispatchProcessor
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        // Outer envelope must match FastAPI aliases (jobId, callbackUrl, userLearningContext).
        // Nested DTOs keep explicit [JsonPropertyName] for snake_case (user_id, completed_sessions, …).
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public async Task TryPostToDesignerAsync(Guid jobId, CancellationToken cancellationToken)
    {
        var job = await sessionDesignJobRepository.GetByIdAsync(jobId, cancellationToken);
        if (job is null || job.Status != SessionDesignJobStatus.Running)
        {
            return;
        }

        var designer = designerOptions.Value;
        var publicBase = publicApiOptions.Value.BaseUrl.TrimEnd('/');
        if (string.IsNullOrEmpty(publicBase))
        {
            logger.LogError("PublicApi:BaseUrl is not configured; failing job {JobId}", jobId);
            FailJob(job, "configuration_error", "PublicApi:BaseUrl is not configured.");
            await sessionDesignJobRepository.SaveChangesAsync(cancellationToken);
            return;
        }

        var callbackUrl = $"{publicBase}/internal/session-design/jobs/{jobId}/callback";

        UserLearningContextDto context;
        try
        {
            context = await userLearningContextFactory.BuildAsync(job.UserId, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to build user learning context for job {JobId}", jobId);
            FailJob(job, "context_build_failed", "Could not build learning context for the user.");
            await sessionDesignJobRepository.SaveChangesAsync(cancellationToken);
            return;
        }

        var client = httpClientFactory.CreateClient("SessionDesigner");
        var payload = new DesignJobRequestDto
        {
            JobId = jobId,
            CallbackUrl = callbackUrl,
            UserLearningContext = context
        };

        HttpResponseMessage response;
        try
        {
            response = await client.PostAsJsonAsync("v1/design-jobs", payload, SerializerOptions, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "HTTP error calling session designer for job {JobId}", jobId);
            FailJob(job, "designer_unreachable", "Session designer service could not be reached.");
            await sessionDesignJobRepository.SaveChangesAsync(cancellationToken);
            return;
        }

        if (response.StatusCode != HttpStatusCode.Accepted)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogWarning(
                "Session designer returned {Status} for job {JobId}: {Body}",
                (int)response.StatusCode,
                jobId,
                body);
            FailJob(
                job,
                "designer_reject",
                $"Session designer did not accept the job (HTTP {(int)response.StatusCode}).");
            await sessionDesignJobRepository.SaveChangesAsync(cancellationToken);
        }
    }

    private static void FailJob(SessionDesignJob job, string? code, string message)
    {
        job.MarkFailed(code, message, DateTimeOffset.UtcNow);
    }

    private sealed class DesignJobRequestDto
    {
        public Guid JobId { get; set; }
        public string CallbackUrl { get; set; } = "";
        public UserLearningContextDto UserLearningContext { get; set; } = null!;
    }
}
