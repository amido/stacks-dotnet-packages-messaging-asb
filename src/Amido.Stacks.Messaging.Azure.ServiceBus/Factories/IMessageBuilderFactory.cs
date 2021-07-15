using Amido.Stacks.Messaging.Azure.ServiceBus.Serializers;

namespace Amido.Stacks.Messaging.Azure.ServiceBus.Factories
{
    public interface IMessageBuilderFactory
    {
        IMessageBuilder CreateMessageBuilder(string name);
    }
}