using System;
using System.Runtime.Serialization;
using Microsoft.Azure.ServiceBus;

namespace Amido.Stacks.Messaging.Azure.ServiceBus.Exceptions
{
    [Serializable]
    public class MessageSerializationException : MessageParsingException
    {
        public MessageSerializationException(Guid correlationId, string message,
            Exception innerException = null) : base((int)ExceptionIds.Serialization, correlationId, message,
            innerException)
        {
        }

        public MessageSerializationException(string exceptionMessage, Message context,
            Exception innerException = null) : base((int)ExceptionIds.Serialization, exceptionMessage, context,
            innerException)
        {
        }

        protected MessageSerializationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}