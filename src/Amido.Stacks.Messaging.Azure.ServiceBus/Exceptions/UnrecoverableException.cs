using System;
using System.Runtime.Serialization;

namespace Amido.Stacks.Messaging.Azure.ServiceBus.Exceptions
{
    [Serializable]
    public class UnrecoverableException : ServiceBusExceptionBase
    {
        public UnrecoverableException(int exceptionCode, Guid correlationId, string message,
            Exception innerException = null) : base(exceptionCode, correlationId, message, innerException)
        {
        }

        public UnrecoverableException(int exceptionCode, string message, Exception innerException = null)
            : base(exceptionCode, message, innerException)
        {
        }

        public UnrecoverableException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
