using System.Threading.Tasks;

namespace Amido.Stacks.Messaging.Azure.ServiceBus.Senders
{
    public interface IMessageSender
    {
        string Alias { get; }
        Task SendAsync<T>(T item);
    }

    public interface ITopicSender : IMessageSender { }

    public interface IQueueSender : IMessageSender { }
}
