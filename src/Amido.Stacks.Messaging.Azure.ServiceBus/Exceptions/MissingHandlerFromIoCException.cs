using System;
using System.Runtime.Serialization;
using Microsoft.Azure.ServiceBus;

namespace Amido.Stacks.Messaging.Azure.ServiceBus.Exceptions
{
    [Serializable]
    public class MissingHandlerFromIoCException : MessageParsingException
    {
        public MissingHandlerFromIoCException(Guid correlationId, string message,
            Exception innerException = null) : base((int)ExceptionIds.MissingHandlerFromIoC, correlationId, message,
            innerException)
        {
        }

        public MissingHandlerFromIoCException(string exceptionMessage, Message context,
            Exception innerException = null) : base((int)ExceptionIds.MissingHandlerFromIoC, exceptionMessage, context,
            innerException)
        {
        }

        protected MissingHandlerFromIoCException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}