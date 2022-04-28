namespace Amido.Stacks.Messaging.Azure.ServiceBus.Serializers
{
    public interface ISessionContext
    {
        string SessionId { get; }
    }
}