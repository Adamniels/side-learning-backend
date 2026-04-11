namespace SideLearning.Application.Configuration;

public sealed class SessionDesignerOptions
{
    public const string SectionName = "SessionDesigner";

    public string BaseUrl { get; set; } = "http://localhost:8010";

    public string SharedSecret { get; set; } = "";

    public int HttpTimeoutSeconds { get; set; } = 120;

    public bool EnableWorker { get; set; } = true;
}
