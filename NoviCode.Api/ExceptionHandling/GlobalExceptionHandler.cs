using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace NoviCode.Api.ExceptionHandling;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext,Exception exception,CancellationToken cancellationToken)
    {
        var exceptionResponse = ExceptionMapper.Map(exception);

        if (exceptionResponse.StatusCode == 500)
            logger.LogError(exception, "Unhandled exception");
        else
            logger.LogWarning(exception, "Handled: {Title}", exceptionResponse.Title);

        httpContext.Response.StatusCode = exceptionResponse.StatusCode;

        var problem = new ProblemDetails
        {
            Status = exceptionResponse.StatusCode,
            Title = exceptionResponse.Title,
            Detail = exceptionResponse.Detail,
        };

        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);

        return true;
    }
}
