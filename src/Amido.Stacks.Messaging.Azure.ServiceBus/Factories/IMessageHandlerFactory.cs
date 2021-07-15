using System;

namespace Amido.Stacks.Messaging.Azure.ServiceBus.Factories
{
    public interface IMessageHandlerFactory
    {
        object CreateHandlerFor(Type type);
    }
}