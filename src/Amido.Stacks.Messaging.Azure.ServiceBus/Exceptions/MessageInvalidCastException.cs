using System;
using System.Runtime.Serialization;
using Microsoft.Azure.ServiceBus;

namespace Amido.Stacks.Messaging.Azure.ServiceBus.Exceptions
{
    [Serializable]
    public class MessageInvalidCastException : MessageParsingException
    {
        public MessageInvalidCastException(
            Guid correlationId,
            string message,
            Exception innerException = null)
            : base((int)ExceptionIds.InvalidCast, correlationId, message, innerException)
        {
        }

        public MessageInvalidCastException(
            string exceptionMessage,
            Message context,
            Exception innerException = null)
            : base((int)ExceptionIds.InvalidCast, exceptionMessage, context, innerException)
        {
        }

        protected MessageInvalidCastException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}