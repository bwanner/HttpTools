using System;

namespace Batzill.Server.Core.Exceptions
{
    public class AuthenticationException : OperationException
    {
        public override int StatusCode => 403;
        public override string StatusDescription => "Authentication failed";

        public AuthenticationException(string message)
            : base(message)
        {

        }

        public AuthenticationException(string format, params object[] parameters) 
            : base(format, parameters)
        {

        }
    }
}
