
namespace NoviCode.Application.Exceptions
{
    public sealed class ExternalServiceException : AppException
    {
        public ExternalServiceException(string message, Exception? innerException = null) 
            : base(message, 502, "EXTERNAL_SERVICE", innerException)
        {
        }
    }
}
