namespace SideLearning.Domain.Users;

public sealed class UserInterest
{
    public const int MaxLabelLength = 200;
    public const int MaxContextLength = 1000;

    public string Label { get; private set; } = null!;
    public float Weight { get; private set; }
    public string Context { get; private set; } = null!;

    private UserInterest() { }

    public static UserInterest Create(string label, float weight, string? context)
    {
        if (string.IsNullOrWhiteSpace(label))
        {
            throw new ArgumentException("User interest label cannot be null or whitespace.", nameof(label));
        }

        var normalizedLabel = label.Trim();
        if (normalizedLabel.Length > MaxLabelLength)
        {
            throw new ArgumentOutOfRangeException(nameof(label), $"User interest label cannot exceed {MaxLabelLength} characters.");
        }

        if (weight < 0 || weight > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(weight), "User interest weight must be between 0 and 1.");
        }

        var normalizedContext = string.IsNullOrWhiteSpace(context) ? string.Empty : context.Trim();
        if (normalizedContext.Length > MaxContextLength)
        {
            throw new ArgumentOutOfRangeException(nameof(context), $"User interest context cannot exceed {MaxContextLength} characters.");
        }

        return new UserInterest { Label = normalizedLabel, Weight = weight, Context = normalizedContext };
    }
}
