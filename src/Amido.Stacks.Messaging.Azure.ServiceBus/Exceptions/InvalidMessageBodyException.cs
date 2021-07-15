using System;
using System.Runtime.Serialization;

namespace Amido.Stacks.Messaging.Azure.ServiceBus.Exceptions
{
    [Serializable]
    public class InvalidMessageBodyException : UnrecoverableException
    {
        public InvalidMessageBodyException(Guid correlationId, string message, Exception innerException = null) : base(
            (int) ExceptionIds.InvalidMessageBody, correlationId, message, innerException)
        {
        }

        public InvalidMessageBodyException(string message, Exception innerException = null) : base(
            (int) ExceptionIds.InvalidMessageBody, message, innerException)
        {
        }

        protected InvalidMessageBodyException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}