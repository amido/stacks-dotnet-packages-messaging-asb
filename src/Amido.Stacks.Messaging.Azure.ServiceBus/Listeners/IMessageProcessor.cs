using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;

namespace Amido.Stacks.Messaging.Azure.ServiceBus.Listeners
{
    public interface IMessageProcessor
    {
        Task ProcessAsync(Message message, CancellationToken cancellationToken);
    }
}
