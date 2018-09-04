using System;

namespace Batzill.Server.Core.Exceptions
{
    public class UnauthorizedException : OperationException
    {
        public override int StatusCode => 401;
        public override string StatusDescription => "Unauthorized";

        public UnauthorizedException(string message)
            : base(message)
        {

        }

        public UnauthorizedException(string format, params object[] parameters) 
            : base(format, parameters)
        {

        }
    }
}
