using Amido.Stacks.Core.Operations;
using Microsoft.Azure.ServiceBus;

namespace Amido.Stacks.Messaging.Azure.ServiceBus.Serializers
{
    public interface IMessageBuilder
    {
        Message Build<T>(T body);
    }
}
