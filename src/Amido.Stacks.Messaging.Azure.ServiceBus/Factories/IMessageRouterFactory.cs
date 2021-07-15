using Amido.Stacks.Messaging.Azure.ServiceBus.Configuration;
using Amido.Stacks.Messaging.Azure.ServiceBus.Senders.Routers;

namespace Amido.Stacks.Messaging.Azure.ServiceBus.Factories
{
    public interface IMessageRouterFactory
    {
        IMessageRouter Create(MessageRoutingRouterConfiguration configuration);
        IMessageRouter CreateDefault<T>() where T : IMessageRouter;
    }
}