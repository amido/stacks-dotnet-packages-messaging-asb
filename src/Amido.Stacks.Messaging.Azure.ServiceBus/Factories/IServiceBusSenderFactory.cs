using System.Threading.Tasks;
using Amido.Stacks.Messaging.Azure.ServiceBus.Configuration;
using Amido.Stacks.Messaging.Azure.ServiceBus.Senders;

namespace Amido.Stacks.Messaging.Azure.ServiceBus.Factories
{
    public interface IServiceBusSenderFactory
    {
        Task<IMessageSender> CreateAsync(ServiceBusSenderEntityConfiguration configuration);
    }
}