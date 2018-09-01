using System;

namespace Batzill.Server.Core.Exceptions
{
    public class NotFoundException : OperationException
    {
        public override int StatusCode => 404;
        public override string StatusDescription => "Not Found";

        public NotFoundException(string message)
            : base(message)
        {

        }

        public NotFoundException(string format, params object[] parameters) 
            : base(format, parameters)
        {

        }
    }
}
