namespace SideLearning.Application.Features.SessionDesign.Callback;

/// <param name="RawBody">Full JSON body from the agent (stored on the job for audit).</param>
public sealed record ProcessSessionDesignCallbackCommand(Guid JobId, string RawBody);
