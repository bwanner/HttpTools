using System;

namespace Batzill.Server.Core.Exceptions
{
    public class InternalServerErrorException : OperationException
    {
        public override int StatusCode => 500;
        public override string StatusDescription => "Internal Server Error";

        public InternalServerErrorException() : this("An Internal Server Error Occured.")
        {
        }

        public InternalServerErrorException(string message)
            : base(message)
        {

        }

        public InternalServerErrorException(string format, params object[] parameters) 
            : base(format, parameters)
        {

        }
    }
}
