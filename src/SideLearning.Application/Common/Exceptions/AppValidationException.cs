namespace SideLearning.Application.Common.Exceptions;

public sealed class AppValidationException : AppException
{
    public AppValidationException(IDictionary<string, string[]> errors)
        : base("validation_failed", "One or more validation failures occurred.")
    {
        Errors = errors;
    }

    public IDictionary<string, string[]> Errors { get; }

    public override int StatusCode => 400;
}
