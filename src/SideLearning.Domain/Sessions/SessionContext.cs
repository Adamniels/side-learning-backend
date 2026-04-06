namespace SideLearning.Domain.Sessions;

public sealed class SessionContext
{
    public string Explanation { get; private set; } = null!;
    public string WhyItMatters { get; private set; } = null!;
    public string? YoutubeUrl { get; private set; } = null!;
    public string? AdditionalResources { get; private set; }

    private SessionContext() { }

    public SessionContext(string explanation, string whyItMatters, string? youtubeUrl, string? additionalResources)
    {
        if (string.IsNullOrWhiteSpace(explanation))
        {
            throw new ArgumentException("Explanation cannot be null or whitespace.", nameof(explanation));
        }

        if (string.IsNullOrWhiteSpace(whyItMatters))
        {
            throw new ArgumentException("Why it matters cannot be null or whitespace.", nameof(whyItMatters));
        }

        Explanation = explanation;
        WhyItMatters = whyItMatters;
        YoutubeUrl = youtubeUrl;
        AdditionalResources = additionalResources;

    }
}

