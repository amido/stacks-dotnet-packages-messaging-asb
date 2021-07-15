using System;
using System.Runtime.Serialization;

namespace Amido.Stacks.Messaging.Azure.ServiceBus.Exceptions
{
    [Serializable]
    public class MessageSenderNotDefinedException : ServiceBusExceptionBase
    {
        public MessageSenderNotDefinedException(Guid correlationId, string message, Exception innerException = null) : base((int)ExceptionIds.MessageSenderNotDefined, correlationId, message, innerException)
        {
        }

        public MessageSenderNotDefinedException(string message, Exception innerException = null) : base((int)ExceptionIds.MessageSenderNotDefined, message, innerException)
        {
        }
    }
}