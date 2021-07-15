using Microsoft.Azure.ServiceBus;

namespace Amido.Stacks.Messaging.Azure.ServiceBus.Serializers
{
    public interface IMessageReader
    {
        T Read<T>(Message message);
    }
}