using System;
using System.Linq;
using Amido.Stacks.Application.CQRS.ApplicationEvents;
using Amido.Stacks.Application.CQRS.Commands;
using Amido.Stacks.Core.Operations;
using Amido.Stacks.Messaging.Azure.ServiceBus.Configuration;
using Amido.Stacks.Messaging.Azure.ServiceBus.Listeners;
using Amido.Stacks.Messaging.Azure.ServiceBus.Validators;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Amido.Stacks.Messaging.Azure.ServiceBus.Factories
{
    public class MessageProcessorFactory : IMessageProcessorFactory
    {
        private readonly IServiceProvider serviceProvider;

        public MessageProcessorFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public IMessageProcessor Create(ServiceBusQueueListenerConfiguration configuration)
        {
            if (!string.IsNullOrEmpty(configuration.MessageProcessor))
            {
                return CreateCustomMessageProcessor(configuration);
            }

            if (configuration is ServiceBusSubscriptionListenerConfiguration)
            {
                return new ServiceBusListenerMessageProcessor<IApplicationEvent>(
                    serviceProvider.GetRequiredService<ILogger<ServiceBusListenerMessageProcessor<IApplicationEvent>>>(),
                    serviceProvider.GetRequiredService<IMessagerReaderFactory>(),
                    serviceProvider.GetRequiredService<IMessageHandlerFactory>(),
                    serviceProvider.GetRequiredService<IValidator<IOperationContext>>(),
                    configuration);
            }
            else
            {
                return new ServiceBusListenerMessageProcessor<ICommand>(
                    serviceProvider.GetRequiredService<ILogger<ServiceBusListenerMessageProcessor<ICommand>>>(),
                    serviceProvider.GetRequiredService<IMessagerReaderFactory>(),
                    serviceProvider.GetRequiredService<IMessageHandlerFactory>(),
                    serviceProvider.GetRequiredService<IValidator<IOperationContext>>(),
                    configuration);
            }
        }

        private IMessageProcessor CreateCustomMessageProcessor(ServiceBusQueueListenerConfiguration configuration)
        {
            IMessageProcessor processor = null;

            var type = Type.GetType(configuration.MessageProcessor);

            if (type != null)
            {
                processor = this.serviceProvider.GetService(type) as IMessageProcessor;
            }

            if (processor == null)
            {
                var processors = this.serviceProvider.GetServices<IMessageProcessor>();
                processor = processors.SingleOrDefault(p => p.GetType().FullName == configuration.MessageProcessor);
            }

            if (processor == null)
            {
                throw new Exception($"The message processor '{configuration.MessageProcessor}' defined for entity '{configuration.Name}' couldn't be resolved. " +
                    $"Make sure the custom message processor has been registered before calling '.AddServiceBus()' and the name provided is the 'FullName' of the class " +
                    $"implementing '{typeof(IMessageProcessor).FullName}' interface");
            }

            return processor;
        }
    }

}
