namespace NoviCode.Application.Exceptions;

public sealed class NotFoundException : AppException
{
    public NotFoundException(string message)
        : base(message, 404, "NOT_FOUND")
    {
    }
}
