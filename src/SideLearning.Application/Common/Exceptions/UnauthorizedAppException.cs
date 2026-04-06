namespace SideLearning.Application.Common.Exceptions;

public sealed class UnauthorizedAppException : AppException
{
    public UnauthorizedAppException(string code, string message) : base(code, message)
    {
    }

    public override int StatusCode => 401;
}
