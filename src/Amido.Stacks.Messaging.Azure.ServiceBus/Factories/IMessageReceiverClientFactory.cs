using System.Threading.Tasks;
using Amido.Stacks.Messaging.Azure.ServiceBus.Configuration;
using Microsoft.Azure.ServiceBus.Core;

namespace Amido.Stacks.Messaging.Azure.ServiceBus.Factories
{
    public interface IMessageReceiverClientFactory
    {
        Task<IReceiverClient> CreateReceiverClient(ServiceBusEntityConfiguration configuration);
    }
}
