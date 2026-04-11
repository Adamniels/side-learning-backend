namespace SideLearning.Application.Abstractions.SessionDesign;

/// <summary>
/// Posts an accepted job to the external session designer service (expects HTTP 202).
/// </summary>
public interface ISessionDesignJobDispatchProcessor
{
    /// <summary>
    /// If the designer service does not return 202, marks the job Failed.
    /// </summary>
    Task TryPostToDesignerAsync(Guid jobId, CancellationToken cancellationToken);
}
