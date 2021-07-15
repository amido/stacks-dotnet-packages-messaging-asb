using System.Threading.Tasks;
using Amido.Stacks.Configuration;
using Amido.Stacks.Messaging.Azure.ServiceBus.Configuration;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;

namespace Amido.Stacks.Messaging.Azure.ServiceBus.Factories
{
    public class ServiceBusClientFactory : IMessageSenderClientFactory, IMessageReceiverClientFactory
    {
        public ServiceBusClientFactory(ISecretResolver<string> secretResolver)
        {
            this.SecretResolver = secretResolver;
        }

        public ISecretResolver<string> SecretResolver { get; }

        public async Task<ISenderClient> CreateSenderClient(ServiceBusEntityConfiguration configuration)
        {
            if (configuration.GetType() == typeof(ServiceBusQueueConfiguration))
                return await CreateQueueClient((ServiceBusQueueConfiguration)configuration);
            else
                return await CreateTopicClient((ServiceBusTopicConfiguration)configuration);
        }

        public async Task<IReceiverClient> CreateReceiverClient(ServiceBusEntityConfiguration configuration)
        {
            if (configuration.GetType() == typeof(ServiceBusSubscriptionListenerConfiguration))
                return await CreateSubscriptionClient((ServiceBusSubscriptionListenerConfiguration)configuration);
            else
                return await CreateQueueClient((ServiceBusQueueListenerConfiguration)configuration);
        }

        private async Task<QueueClient> CreateQueueClient(ServiceBusEntityConfiguration configuration)
        {
            return new QueueClient(
                connectionString: await GetConnectionString(configuration.ConnectionStringSecret),
                entityPath: configuration.Name,
                retryPolicy: GetRetryPolicy()
            );
        }

        private async Task<TopicClient> CreateTopicClient(ServiceBusTopicConfiguration configuration)
        {
            return new TopicClient(
                connectionString: await GetConnectionString(configuration.ConnectionStringSecret),
                entityPath: configuration.Name,
                retryPolicy: GetRetryPolicy()
            );
        }

        private async Task<SubscriptionClient> CreateSubscriptionClient(ServiceBusSubscriptionListenerConfiguration configuration)
        {
            return new SubscriptionClient(
                connectionString: await GetConnectionString(configuration.ConnectionStringSecret),
                topicPath: configuration.Name,
                subscriptionName: configuration.SubscriptionName,
                retryPolicy: GetRetryPolicy()
            );
        }

        private async Task<string> GetConnectionString(Secret connectionStringSecret)
        {
            return await SecretResolver.ResolveSecretAsync(connectionStringSecret);
        }

        private RetryPolicy GetRetryPolicy()
        {
            //TODO: pass the configuration to setup the rety policies
            return RetryPolicy.Default;
        }
    }
}
