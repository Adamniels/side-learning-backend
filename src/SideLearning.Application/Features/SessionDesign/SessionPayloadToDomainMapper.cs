using SideLearning.Application.Features.SessionDesign.Contracts;
using SideLearning.Domain.Sessions;

namespace SideLearning.Application.Features.SessionDesign;

internal static class SessionPayloadToDomainMapper
{
    private const string DefaultWhyItMatters = "Aligned with your learning goals.";

    public static Session CreateSessionFromPayload(Guid userId, SessionPayloadDto payload)
    {
        var explanation = payload.Context.Trim();
        var whyItMatters = string.IsNullOrWhiteSpace(payload.DifficultyAlignment)
            ? DefaultWhyItMatters
            : payload.DifficultyAlignment.Trim();

        var sessionContext = new SessionContext(explanation, whyItMatters, null, null);

        var expectedOutput = string.IsNullOrWhiteSpace(payload.HandsOnExpectedOutput)
            ? null
            : payload.HandsOnExpectedOutput.Trim();

        var handsOn = new SessionHandsOn(payload.HandsOn.Trim(), expectedOutput);
        var reflection = new SessionReflection(null, null, null, null);

        var subjectAreas = payload.SubjectAreas
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(SessionSubjectArea.Create)
            .ToList();

        return Session.Create(
            Guid.NewGuid(),
            userId,
            payload.Title.Trim(),
            payload.Summary.Trim(),
            payload.Goal.Trim(),
            payload.Extension.Trim(),
            sessionContext,
            handsOn,
            reflection,
            subjectAreas,
            payload.EstimatedDurationInMinutes);
    }
}
