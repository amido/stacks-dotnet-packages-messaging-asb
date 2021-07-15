using System;
using System.Runtime.Serialization;
using Microsoft.Azure.ServiceBus;

namespace Amido.Stacks.Messaging.Azure.ServiceBus.Exceptions
{
    [Serializable]
    public class MissingEnclosedMessageTypeException : MessageParsingException
    {
        public MissingEnclosedMessageTypeException(Guid correlationId, string message,
            Exception innerException = null) : base((int) ExceptionIds.MissingEnclosedMessageType, correlationId,
            message, innerException)
        {
        }

        public MissingEnclosedMessageTypeException(string exceptionMessage, Message context,
            Exception innerException = null) : base((int) ExceptionIds.MissingEnclosedMessageType, exceptionMessage,
            context, innerException)
        {
        }

        protected MissingEnclosedMessageTypeException(SerializationInfo info, StreamingContext context) : base(info,
            context)
        {
        }
    }
}