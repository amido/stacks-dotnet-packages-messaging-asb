using System;
using System.Runtime.Serialization;
using Microsoft.Azure.ServiceBus;

namespace Amido.Stacks.Messaging.Azure.ServiceBus.Exceptions
{
    [Serializable]
    public class MissingHandlerMethodException : MessageParsingException
    {
        public MissingHandlerMethodException(Guid correlationId, string message,
            Exception innerException = null) : base((int)ExceptionIds.MissingHandlerMethod, correlationId, message,
            innerException)
        {
        }

        public MissingHandlerMethodException(string exceptionMessage, Message context,
            Exception innerException = null) : base((int)ExceptionIds.MissingHandlerMethod, exceptionMessage, context,
            innerException)
        {
        }

        protected MissingHandlerMethodException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}