using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoviCode.Application.Exceptions
{
    public sealed class ConcurrencyException : AppException
    {
        public ConcurrencyException(string message, Exception? innerException = null) 
            : base(message, 409, "CONCURRENCY_CONFLICT", innerException)
        {
        }
    }
}
