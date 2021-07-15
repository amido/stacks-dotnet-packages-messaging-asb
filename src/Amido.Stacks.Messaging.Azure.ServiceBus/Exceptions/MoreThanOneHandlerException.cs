using System;
using System.Runtime.Serialization;

namespace Amido.Stacks.Messaging.Azure.ServiceBus.Exceptions
{
    [Serializable]
    public class MoreThanOneHandlerException : UnrecoverableException
    {
        public MoreThanOneHandlerException(Guid correlationId, string message,
            Exception innerException = null) : base((int)ExceptionIds.MoreThanOneHandler, correlationId, message,
            innerException)
        {
        }

        public MoreThanOneHandlerException(string message,
            Exception innerException = null) : base((int)ExceptionIds.MoreThanOneHandler, message,
            innerException)
        {
        }

        protected MoreThanOneHandlerException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}