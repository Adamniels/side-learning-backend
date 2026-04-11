namespace SideLearning.Application.Configuration;

public sealed class PublicApiCallbacksOptions
{
    public const string SectionName = "PublicApi";

    // who is this for? the session designer can't reach this file? or what have i not understood yet?

    /// <summary>Base URL the session designer agent uses to call back (e.g. http://localhost:5207).</summary>
    public string BaseUrl { get; set; } = "";
}
