namespace SideLearning.Domain.Sessions;

public sealed class SessionTopic
{
    public string Value { get; private set; } = null!;

    private SessionTopic() { }

    public static SessionTopic Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Session topic cannot be null or whitespace.", nameof(value));
        }

        return new SessionTopic { Value = value.Trim() };
    }
}
