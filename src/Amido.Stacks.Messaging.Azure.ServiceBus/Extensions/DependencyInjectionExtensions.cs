using System;
using System.Linq;
using Amido.Stacks.Application.CQRS.ApplicationEvents;
using Amido.Stacks.Application.CQRS.Commands;
using Amido.Stacks.Core.Operations;
using Amido.Stacks.Messaging.Azure.ServiceBus.Configuration;
using Amido.Stacks.Messaging.Azure.ServiceBus.Extensions;
using Amido.Stacks.Messaging.Azure.ServiceBus.Factories;
using Amido.Stacks.Messaging.Azure.ServiceBus.Hosts;
using Amido.Stacks.Messaging.Azure.ServiceBus.Senders;
using Amido.Stacks.Messaging.Azure.ServiceBus.Senders.Dispatchers;
using Amido.Stacks.Messaging.Azure.ServiceBus.Senders.Publishers;
using Amido.Stacks.Messaging.Azure.ServiceBus.Senders.Routers;
using Amido.Stacks.Messaging.Azure.ServiceBus.Serializers;
using Amido.Stacks.Messaging.Azure.ServiceBus.Validators;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddServiceBus(this IServiceCollection services)
        {
            services.AddSerializers();
            services.AddValidators();

            var configuration = GetConfiguration(services);

            var sendersRegistered = services.AddServiceBusSenders(configuration.Sender);
            var listenersRegistered = services.AddServiceBusListeners(configuration.Listener);

            if (!sendersRegistered && !listenersRegistered)
            {
                throw new Exception("Unable to register any sender or lister for service bus. Make sure the configuration has been setup correctly.");
            }

            return services;
        }

        private static ServiceBusConfiguration GetConfiguration(IServiceCollection services)
        {
            var config = services.BuildServiceProvider()
                .GetService<IOptions<ServiceBusConfiguration>>()
                .Value;

            if (config == null || (config.Sender == null && config.Listener == null))
            {
                throw new System.Exception($"Configuration for '{nameof(IOptions<ServiceBusConfiguration>)}' not found. Ensure the call to 'service.Configure<{nameof(ServiceBusConfiguration)}>(configuration)' was called and the appsettings contains at least a definition for Sender or Listener. ");
            }

            return config;
        }

        private static void AddSerializers(this IServiceCollection services)
        {
            services
                .AddTransient<JsonMessageSerializer>()
                .AddTransient<CloudEventMessageSerializer>();

            services
                .AddTransient<IMessageBuilder, JsonMessageSerializer>()
                .AddTransient<IMessageBuilder, CloudEventMessageSerializer>()
                .AddTransient<IMessageBuilderFactory, MessageBuilderFactory>()
            ;

            services
                .AddTransient<IMessageReader, JsonMessageSerializer>()
                .AddTransient<IMessageReader, CloudEventMessageSerializer>()
                .AddTransient<IMessagerReaderFactory, MessageReaderFactory>()
            ;
        }

        private static void AddValidators(this IServiceCollection services)
        {
            services.AddTransient<IValidator<IOperationContext>, DataAnnotationValidator>();
        }

        // SENDERS
        private static bool AddServiceBusSenders(this IServiceCollection services, ServiceBusSenderConfiguration configuration)
        {
            if (configuration == null ||
                (
                    (configuration.Queues == null || configuration.Queues.Length == 0) &&
                    (configuration.Topics == null || configuration.Topics.Length == 0)
                )
               )
            {
                return false;
            }

            services
                .TryAddTransient<IMessageSenderClientFactory, ServiceBusClientFactory>();
            services
                .AddTransient<IServiceBusSenderFactory, ServiceBusSenderFactory>()
                .AddTransient<IMessageRouterFactory, MessageRouterFactory>()
            ;

            var queuesSenderRegistered = services.AddServiceBusQueueSender(configuration?.Queues);
            var topicSenderRegistered = services.AddServiceBusTopicSender(configuration?.Topics);

            if (!queuesSenderRegistered && !topicSenderRegistered)
            {
                return false;
            }

            services.AddServiceBusMessageRouter(configuration);

            return true;
        }

        private static bool AddServiceBusQueueSender(this IServiceCollection services, ServiceBusQueueConfiguration[] queues)
        {
            if (queues == null || queues.Length == 0)
                return false;

            services
                .AddTransient<ICommandDispatcher, CommandDispatcher>()
            ;

            var factory = services.BuildServiceProvider().GetRequiredService<IServiceBusSenderFactory>();

            foreach (var queue in queues)
            {
                var sender = (IQueueSender)factory.CreateAsync(queue).Result;
                services.AddSingleton(sender);
                services.AddSenderHealthCheck(sender);
            }

            return true;
        }

        private static bool AddServiceBusTopicSender(this IServiceCollection services, ServiceBusTopicConfiguration[] topics)
        {
            if (topics == null || topics.Length == 0)
                return false;

            services
                .AddTransient<IApplicationEventPublisher, EventPublisher>()
            ;

            var factory = services.BuildServiceProvider().GetRequiredService<IServiceBusSenderFactory>();

            foreach (var topic in topics)
            {
                var sender = (ITopicSender)factory.CreateAsync(topic).Result;
                services.AddSingleton(sender);
                services.AddSenderHealthCheck(sender);
            }

            return true;
        }

        private static void AddSenderHealthCheck(this IServiceCollection services, IMessageSender sender)
        {
            services
                .AddHealthChecks()
                .AddCheck($"ServiceBus Sender {sender.Alias}", (IHealthCheck)sender, HealthStatus.Unhealthy, null);
        }

        private static void AddServiceBusMessageRouter(this IServiceCollection services, ServiceBusSenderConfiguration configuration)
        {
            services.AddSingleton<ServiceBusAbstractRouter<ITopicRouter>>();
            services.AddSingleton<ServiceBusAbstractRouter<IQueueRouter>>();

            var factory = services.BuildServiceProvider().GetRequiredService<IMessageRouterFactory>();

            if ((configuration.Routing == null) ||
                (configuration.Routing.Queues == null || configuration.Routing.Queues.Length == 0)
              )
            {
                // This logic will need refactoring if we allow external clients to  inject their own clients

                if (configuration.Queues?.Length == 1)
                {
                    services.AddSingleton<IQueueRouter, DefaultMessageRouter<IQueueSender>>();
                }
                else if (configuration.Queues?.Length > 1)
                {
                    throw new Exception("Multiple queues were configured but no routing logic setup!");
                }
            }
            else
            {
                foreach (var route in configuration.Routing?.Queues ?? Enumerable.Empty<MessageRoutingQueueRouterConfiguration>())
                {
                    services.AddSingleton<IQueueRouter>((IQueueRouter)factory.Create(route));
                }
            }

            if ((configuration.Routing == null) ||
                (configuration.Routing.Topics == null || configuration.Routing.Topics.Length == 0)
              )
            {
                if (configuration.Topics?.Length == 1)
                {
                    services.AddSingleton<ITopicRouter, DefaultMessageRouter<ITopicSender>>();
                }
                else if (configuration.Topics?.Length > 1)
                {
                    throw new Exception("Multiple topics were configured but no routing logic setup!");
                }
            }
            else
            {
                foreach (var route in configuration.Routing?.Topics ?? Enumerable.Empty<MessageRoutingTopicRouterConfiguration>())
                {
                    services.AddSingleton<ITopicRouter>((ITopicRouter)factory.Create(route));
                }
            }
        }

        // LISTENERS
        public static bool AddServiceBusListeners(
            this IServiceCollection services,
            ServiceBusListenerConfiguration configuration)
        {

            if (configuration == null ||
                (
                    (configuration.Queues == null || configuration.Queues.Length == 0) &&
                    (configuration.Topics == null || configuration.Topics.Length == 0)
                )
               )
            {
                return false;
            }

            services
                .TryAddTransient<IMessageReceiverClientFactory, ServiceBusClientFactory>();

            services
                .AddHostedService<ServiceBusListenerHost>()

                .AddSingleton<IMessageHandlerFactory>(p =>
                    new MessageHandlerFactory(p,
                        services.GetRegisteredHandlersFor(
                            typeof(ICommandHandler<,>),
                            typeof(IApplicationEventHandler<>))
                        ))

                .AddSingleton<IMessageProcessorFactory, MessageProcessorFactory>()
                .AddTransient<IServiceBusListenerFactory, ServiceBusListenerFactory>()
            ;

            var queueListenerRegistered = services.AddQueueListeners(configuration.Queues);
            var topicListenerRegistered = services.AddSubscriptionListeners(configuration.Topics);

            return queueListenerRegistered || topicListenerRegistered;
        }

        public static bool AddQueueListeners(this IServiceCollection services, ServiceBusQueueListenerConfiguration[] queues)
        {
            if (queues == null || queues.Length == 0)
                return false;

            var factory = services.BuildServiceProvider().GetRequiredService<IServiceBusListenerFactory>();

            foreach (var queue in queues)
            {
                services.AddSingleton(factory.Create<ICommand>(queue));
            }

            return true;
        }

        public static bool AddSubscriptionListeners(this IServiceCollection services, ServiceBusSubscriptionListenerConfiguration[] topics)
        {
            if (topics == null || topics.Length == 0)
                return false;

            var factory = services.BuildServiceProvider().GetRequiredService<IServiceBusListenerFactory>();

            foreach (var topic in topics)
            {
                services.AddSingleton(factory.Create<IApplicationEvent>(topic));
            }

            return true;
        }
    }
}
