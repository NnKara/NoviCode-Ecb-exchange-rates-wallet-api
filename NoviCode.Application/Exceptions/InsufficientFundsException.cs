
namespace NoviCode.Application.Exceptions
{
    public sealed class InsufficientFundsException : AppException
    {
        public InsufficientFundsException(string message) 
            : base(message, 409, "INSUFFICIENT_FUNDS")
        {
        }
    }
}
