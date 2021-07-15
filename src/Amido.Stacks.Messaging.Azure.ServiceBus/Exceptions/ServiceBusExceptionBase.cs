using System;
using System.Runtime.Serialization;
using Amido.Stacks.Core.Exceptions;

namespace Amido.Stacks.Messaging.Azure.ServiceBus.Exceptions
{
    [Serializable]
    public class ServiceBusExceptionBase : InfrastructureExceptionBase, IException
    {
        public ServiceBusExceptionBase(
            int exceptionCode,
            Guid correlationId,
            string message,
            Exception innerException = null)
            : base(exceptionCode, message)
        {
            (ExceptionCode, CorrelationId)
                = (exceptionCode, correlationId);
        }

        public ServiceBusExceptionBase(
            int exceptionCode,
            string message,
            Exception innerException = null)
            : this(exceptionCode, Guid.NewGuid(), message, innerException)
        {
        }

        protected ServiceBusExceptionBase(SerializationInfo info, StreamingContext context)
            : base(0, null)
        {
        }

        public Guid CorrelationId { get; protected set; }
        public override int ExceptionCode { get; protected set; }
    }
}