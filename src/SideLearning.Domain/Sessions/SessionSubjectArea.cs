namespace SideLearning.Domain.Sessions;

public sealed class SessionSubjectArea
{
    public string Value { get; private set; } = null!;

    private SessionSubjectArea() { }

    public static SessionSubjectArea Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Session subject area cannot be null or whitespace.", nameof(value));
        }

        return new SessionSubjectArea { Value = value.Trim() };
    }
}
