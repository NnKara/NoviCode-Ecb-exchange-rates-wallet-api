
namespace NoviCode.Application.Exceptions
{
    public abstract class AppException : Exception
    {

        public int StatusCode { get; }
        public string? ErrorCode { get; }

        protected AppException(string message, int statusCode, string? errorCode = null, Exception? innerException = null) 
            : base(message, innerException)
        {
            StatusCode = statusCode;
            ErrorCode = errorCode;
        }

    }
}
