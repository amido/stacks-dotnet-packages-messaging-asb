using System;
using Amido.Stacks.Application.CQRS.ApplicationEvents;

namespace Amido.Stacks.Messaging.Events
{
    public class DummyEvent : IApplicationEvent
    {
        public DummyEvent() { }

        public DummyEvent(Guid correlationId)
        {
            this.CorrelationId = correlationId;
        }

        public int EventCode => 9871;

        public int OperationCode { get; }

        public Guid CorrelationId { get; }
    }
}
