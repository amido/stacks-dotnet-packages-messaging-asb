using System;

namespace Amido.Stacks.Messaging.Azure.ServiceBus.Serializers
{
    public interface ICloudEvent
    {
        string Id { get; }
        string Subject { get; }
        DateTime? Time { get; }
    }
}
