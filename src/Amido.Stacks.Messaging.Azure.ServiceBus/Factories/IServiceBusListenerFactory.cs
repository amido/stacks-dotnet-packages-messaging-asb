using Amido.Stacks.Messaging.Azure.ServiceBus.Configuration;
using Amido.Stacks.Messaging.Azure.ServiceBus.Listeners;

namespace Amido.Stacks.Messaging.Azure.ServiceBus.Factories
{
    public interface IServiceBusListenerFactory
    {
        IMessageListener Create<T>(ServiceBusQueueListenerConfiguration configuration);
    }
}