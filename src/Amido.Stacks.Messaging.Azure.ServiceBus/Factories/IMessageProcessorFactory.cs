using Amido.Stacks.Messaging.Azure.ServiceBus.Configuration;
using Amido.Stacks.Messaging.Azure.ServiceBus.Listeners;

namespace Amido.Stacks.Messaging.Azure.ServiceBus.Factories
{
    public interface IMessageProcessorFactory
    {
        IMessageProcessor Create(ServiceBusQueueListenerConfiguration configuration);
    }
}
