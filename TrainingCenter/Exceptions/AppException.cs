using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainingCenter.Core.Exceptions
{
    public class AppException : Exception
    {
        public int StatusCode { get; }

        public AppException(string message, int statusCode = 400)
            : base(message)
        {
            StatusCode = statusCode;
        }
    }

    public class BadRequestException : AppException
    {
        public BadRequestException(string message)
            :base (message, 400) { }
    }

    public class NotFoundException : AppException
    {
        public NotFoundException(string message)
            : base(message, 404) { }
    }

    public class ConflictException : AppException
    {
        public ConflictException(string message)
            : base(message, 409) { }
    }

    public class UnauthorizedException : AppException
    {
        public UnauthorizedException(string message = "Unauthorized")
            : base(message, 401) { }
    }

    public class ForbiddenException : AppException
    {
        public ForbiddenException(string message = "Forbidden") 
            : base(message, 403) { }
    }

    public class InternalServerErrorException : AppException
    {
        public InternalServerErrorException(string message = "Internal Server Error")
            : base(message, 500) { }
    }
}
