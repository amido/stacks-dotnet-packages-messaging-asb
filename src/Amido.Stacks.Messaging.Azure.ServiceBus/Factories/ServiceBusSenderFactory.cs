using System;
using System.Threading.Tasks;
using Amido.Stacks.Messaging.Azure.ServiceBus.Configuration;
using Amido.Stacks.Messaging.Azure.ServiceBus.Senders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Amido.Stacks.Messaging.Azure.ServiceBus.Factories
{
    public class ServiceBusSenderFactory : IServiceBusSenderFactory
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IMessageSenderClientFactory messageSenderClientFactory;
        private readonly IMessageBuilderFactory builderFactory;

        public ServiceBusSenderFactory(
            IServiceProvider serviceProvider,
            IMessageSenderClientFactory messageSenderClientFactory,
            IMessageBuilderFactory builderFactory)
        {
            this.serviceProvider = serviceProvider;
            this.messageSenderClientFactory = messageSenderClientFactory;
            this.builderFactory = builderFactory;
        }

        public async Task<IMessageSender> CreateAsync(ServiceBusSenderEntityConfiguration configuration)
        {
            return new ServiceBusMessageSender(
                      this.serviceProvider.GetRequiredService<ILogger<ServiceBusMessageSender>>(),
                      await this.messageSenderClientFactory.CreateSenderClient(configuration),
                      builderFactory.CreateMessageBuilder(configuration.Serializer),
                      configuration
                   );
        }
    }
}