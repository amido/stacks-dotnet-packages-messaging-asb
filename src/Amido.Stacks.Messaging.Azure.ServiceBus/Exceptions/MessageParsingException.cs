using System;
using System.Runtime.Serialization;
using Microsoft.Azure.ServiceBus;

namespace Amido.Stacks.Messaging.Azure.ServiceBus.Exceptions
{
    [Serializable]
    public class MessageParsingException : UnrecoverableException
    {
        public MessageParsingException(int exceptionCode, Guid correlationId, string message,
            Exception innerException = null) :
            base(exceptionCode, correlationId, message, innerException)
        {
        }

        public MessageParsingException(int exceptionCode, string exceptionMessage, Message context,
            Exception innerException = null) :
            this(exceptionCode, new Guid(context?.CorrelationId ?? Guid.Empty.ToString()), exceptionMessage,
                innerException)
        {
            (ContentType, MessageId, SessionId)
                = (context?.ContentType, context?.MessageId, context?.SessionId);
        }

        protected MessageParsingException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public string ContentType { get; }
        public string MessageId { get; }
        public string SessionId { get; }
    }
}