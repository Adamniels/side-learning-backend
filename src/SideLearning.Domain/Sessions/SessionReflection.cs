namespace SideLearning.Domain.Sessions;

public sealed class SessionReflection
{
    public string? Solution { get; private set; }
    public string? Reflection { get; private set; }
    public string? Notes { get; private set; }
    public SessionDifficultyFeedback? DifficultyFeedback { get; private set; }

    private SessionReflection() { }

    public SessionReflection(
        string? solution,
        string? reflection,
        string? notes,
        SessionDifficultyFeedback? difficultyFeedback)
    {
        Solution = solution;
        Reflection = reflection;
        Notes = notes;
        DifficultyFeedback = difficultyFeedback;
    }
}
