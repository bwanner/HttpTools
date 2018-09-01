using System;

namespace Batzill.Server.Core.Exceptions
{
    public class BadRequestException : OperationException
    {
        public override int StatusCode => 400;
        public override string StatusDescription => "Bad Request";

        public BadRequestException(string message)
            : base(message)
        {

        }

        public BadRequestException(string format, params object[] parameters) 
            : base(format, parameters)
        {

        }
    }
}
