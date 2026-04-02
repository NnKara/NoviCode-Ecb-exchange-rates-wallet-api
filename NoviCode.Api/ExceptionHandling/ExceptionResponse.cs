namespace NoviCode.Api.ExceptionHandling;

public sealed record ExceptionResponse(int StatusCode, string Title, string Detail);
