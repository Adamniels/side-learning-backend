namespace SideLearning.Application.Common.Exceptions;

public sealed class NotFoundException : AppException
{
    public NotFoundException(string code, string message) : base(code, message)
    {
    }

    public override int StatusCode => 404;
}
