using System.Threading.Tasks;

namespace Amido.Stacks.Messaging.Azure.ServiceBus.Listeners
{
    public interface IMessageListener
    {
        Task StartAsync();
        Task StopAsync();
    }
}
