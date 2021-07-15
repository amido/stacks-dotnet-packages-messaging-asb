using Amido.Stacks.Application.CQRS.ApplicationEvents;
using Amido.Stacks.Application.CQRS.Commands;
using Amido.Stacks.Messaging.Azure.ServiceBus.Listeners;

namespace Amido.Stacks.Messaging.Azure.ServiceBus
{
    internal static class Constants
    {
        internal static class Defaults
        {
            public const string CommandSerializer = nameof(Serializers.JsonMessageSerializer);
            public const string EventSerializer = nameof(Serializers.CloudEventMessageSerializer);

            public const string QueueListenerMessageProcessor = nameof(ServiceBusListenerMessageProcessor<ICommand>);
            public const string TopicListenerMessageProcessor = nameof(ServiceBusListenerMessageProcessor<IApplicationEvent>);
        }
    }

    public enum MessageProperties
    {
        EnclosedMessageType,
        Serializer
    }
}
