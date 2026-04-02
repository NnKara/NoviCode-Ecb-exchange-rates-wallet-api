using NoviCode.Application.Exceptions;

namespace NoviCode.Api.ExceptionHandling;

public static class ExceptionMapper
{
    public static ExceptionResponse Map(Exception exception)
    {
        return exception switch
        {
            NotFoundException ex => new(ex.StatusCode, "Not Found", ex.Message),
            ValidationException ex => new(ex.StatusCode, "Bad Request", ex.Message),
            InsufficientFundsException ex => new(ex.StatusCode, "Insufficient funds", ex.Message),
            ExternalServiceException ex => new(ex.StatusCode, "Bad Gateway", ex.Message),
            AppException ex => new(ex.StatusCode, "Error", ex.Message),
            _ => new(500, "Server Error", "An unexpected error occurred."),
        };
    }
}
