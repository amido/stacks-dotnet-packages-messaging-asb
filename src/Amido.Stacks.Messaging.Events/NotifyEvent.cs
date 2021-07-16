using System;
using Amido.Stacks.Application.CQRS.ApplicationEvents;
using Amido.Stacks.Messaging.Azure.ServiceBus.Serializers;

namespace Amido.Stacks.Messaging.Events
{
    public class NotifyEvent : IApplicationEvent, ICloudEvent
    {
        public int EventCode => 123;
        public Guid CorrelationId { get; }
        public int OperationCode { get; }
        public string Id { get; } = Guid.NewGuid().ToString();
        public DateTime? Time { get; } = DateTime.UtcNow;
        public string Subject { get; set; }

        public NotifyEvent(Guid correlationId, int operationCode, string subject = null)
        {
            this.CorrelationId = correlationId;
            this.OperationCode = operationCode;
            this.Subject = subject;
        }
    }
}
