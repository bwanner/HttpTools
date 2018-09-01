using System;

namespace Batzill.Server.Core.Exceptions
{
    public abstract class OperationException : Exception
    {
        public abstract int StatusCode
        {
            get;
        }
        public abstract string StatusDescription
        {
            get;
        }

        public OperationException(string message) 
            : base(message)
        {
        }
        
        public OperationException(string format, params object[] parameters) 
            : this(parameters == null || parameters.Length == 0 ? format : string.Format(format, parameters))
        {
        }
    }
}
