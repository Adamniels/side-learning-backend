using System.Text.Json;
using Microsoft.Extensions.Logging;
using SideLearning.Application.Abstractions.SessionDesign;
using SideLearning.Application.Abstractions.Sessions;
using SideLearning.Application.Features.SessionDesign.Contracts;
using SideLearning.Domain.SessionDesign;

namespace SideLearning.Application.Features.SessionDesign.Callback;

public sealed class ProcessSessionDesignCallbackCommandHandler(
    ISessionDesignJobRepository sessionDesignJobRepository,
    ISessionRepository sessionRepository,
    ILogger<ProcessSessionDesignCallbackCommandHandler> logger)
{
    private static readonly JsonSerializerOptions DeserializeOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task HandleAsync(ProcessSessionDesignCallbackCommand command, CancellationToken cancellationToken)
    {
        SessionDesignCallbackBodyDto? body;
        try
        {
            body = JsonSerializer.Deserialize<SessionDesignCallbackBodyDto>(command.RawBody, DeserializeOptions);
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Invalid JSON in session design callback for job {JobId}", command.JobId);
            return;
        }

        if (body is null)
        {
            logger.LogWarning("Empty session design callback body for job {JobId}", command.JobId);
            return;
        }

        var job = await sessionDesignJobRepository.GetByIdAsync(command.JobId, cancellationToken);
        if (job is null)
        {
            logger.LogWarning("Session design callback for unknown job {JobId}", command.JobId);
            return;
        }

        if (job.Status is SessionDesignJobStatus.Succeeded or SessionDesignJobStatus.Failed)
        {
            logger.LogDebug("Ignoring duplicate callback for terminal job {JobId} status {Status}", job.Id, job.Status);
            LogIfConflictingOutcome(job, body);
            return;
        }

        if (job.Status != SessionDesignJobStatus.Running)
        {
            logger.LogWarning(
                "Session design callback for job {JobId} in unexpected status {Status}",
                job.Id,
                job.Status);
            return;
        }

        var outcome = body.Outcome.Trim().ToLowerInvariant();
        if (outcome == "succeeded")
        {
            SessionPayloadDto? payload = body.SessionDesignResult?.SessionPayload;
            if (payload is null)
            {
                try
                {
                    using var doc = JsonDocument.Parse(command.RawBody);
                    if (doc.RootElement.TryGetProperty("sessionDesignResult", out var sdr)
                        && sdr.TryGetProperty("session_payload", out var sp))
                    {
                        payload = JsonSerializer.Deserialize<SessionPayloadDto>(sp.GetRawText(), DeserializeOptions);
                    }
                }
                catch (JsonException)
                {
                    // handled below
                }
            }

            if (payload is null)
            {
                logger.LogWarning("Succeeded callback missing session payload for job {JobId}", job.Id);
                job.MarkFailed(
                    "invalid_callback",
                    "Succeeded outcome requires session design payload.",
                    DateTimeOffset.UtcNow);
                await sessionDesignJobRepository.SaveChangesAsync(cancellationToken);
                return;
            }

            var session = SessionPayloadToDomainMapper.CreateSessionFromPayload(job.UserId, payload);
            await sessionRepository.AddAsync(session, cancellationToken);

            job.MarkSucceeded(command.RawBody, session.Id, DateTimeOffset.UtcNow);
            await sessionDesignJobRepository.SaveChangesAsync(cancellationToken);
            return;
        }

        if (outcome == "failed")
        {
            var message = body.Error?.Message?.Trim();
            if (string.IsNullOrEmpty(message))
            {
                message = "Session design failed.";
            }

            job.MarkFailed(body.Error?.Code, message, DateTimeOffset.UtcNow);
            await sessionDesignJobRepository.SaveChangesAsync(cancellationToken);
            return;
        }

        logger.LogWarning("Session design callback for job {JobId} has unknown outcome {Outcome}", job.Id, body.Outcome);
        job.MarkFailed("invalid_callback", "Unknown callback outcome.", DateTimeOffset.UtcNow);
        await sessionDesignJobRepository.SaveChangesAsync(cancellationToken);
    }

    private void LogIfConflictingOutcome(SessionDesignJob job, SessionDesignCallbackBodyDto body)
    {
        var incoming = body.Outcome.Trim().ToLowerInvariant();
        var expected = job.Status == SessionDesignJobStatus.Succeeded ? "succeeded" : "failed";
        if (incoming != expected)
        {
            logger.LogWarning(
                "Duplicate callback for job {JobId} reported outcome {Incoming} but job is already {Expected}",
                job.Id,
                incoming,
                expected);
        }
    }
}
