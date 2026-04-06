namespace SideLearning.Domain.Sessions;

public sealed class SessionHandsOn
{
    public string Instructions { get; private set; } = null!;
    public string? ExpectedOutput { get; private set; }

    private SessionHandsOn() { }

    public SessionHandsOn(string instructions, string? expectedOutput)
    {
        if (string.IsNullOrWhiteSpace(instructions))
        {
            throw new ArgumentException("Instructions cannot be null or whitespace.", nameof(instructions));
        }

        Instructions = instructions;
        ExpectedOutput = expectedOutput;
    }
}
