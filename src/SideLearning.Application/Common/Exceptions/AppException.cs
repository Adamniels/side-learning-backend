namespace SideLearning.Application.Common.Exceptions;

public abstract class AppException : Exception
{
    protected AppException(string code, string message) : base(message)
    {
        Code = code;
    }

    public string Code { get; }
    public abstract int StatusCode { get; }
}
