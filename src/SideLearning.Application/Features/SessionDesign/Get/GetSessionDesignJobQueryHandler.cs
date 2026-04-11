using System.Text.Json;
using SideLearning.Application.Abstractions.SessionDesign;
using SideLearning.Application.Common.Exceptions;
using SideLearning.Domain.SessionDesign;

namespace SideLearning.Application.Features.SessionDesign.Get;

public sealed class GetSessionDesignJobQueryHandler(ISessionDesignJobRepository sessionDesignJobRepository)
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task<SessionDesignJobResponseDto> HandleAsync(Guid userId, Guid jobId, CancellationToken cancellationToken)
    {
        var job = await sessionDesignJobRepository.GetByIdForUserAsync(userId, jobId, cancellationToken);
        if (job is null)
        {
            throw new NotFoundException("session_design_job_not_found", "Session design job was not found.");
        }

        object? resultObject = null;
        if (job.Status == SessionDesignJobStatus.Succeeded && !string.IsNullOrEmpty(job.ResultJson))
        {
            try
            {
                using var doc = JsonDocument.Parse(job.ResultJson);
                if (doc.RootElement.TryGetProperty("sessionDesignResult", out var inner))
                {
                    resultObject = JsonSerializer.Deserialize<object>(inner.GetRawText(), JsonOptions);
                }
                else
                {
                    resultObject = JsonSerializer.Deserialize<object>(job.ResultJson, JsonOptions);
                }
            }
            catch (JsonException)
            {
                resultObject = null;
            }
        }

        SessionDesignJobErrorDto? err = null;
        if (job.Status == SessionDesignJobStatus.Failed)
        {
            err = new SessionDesignJobErrorDto
            {
                Code = job.ErrorCode,
                Message = job.ErrorMessage ?? "Session design failed."
            };
        }

        return new SessionDesignJobResponseDto
        {
            JobId = job.Id,
            Status = job.Status.ToString().ToLowerInvariant(),
            CreatedAtUtc = job.CreatedAtUtc,
            StartedAtUtc = job.StartedAtUtc,
            CompletedAtUtc = job.CompletedAtUtc,
            CreatedSessionId = job.CreatedSessionId,
            SessionDesignResult = resultObject,
            Error = err
        };
    }
}
