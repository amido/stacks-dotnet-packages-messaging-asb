using System;
using System.Collections.Generic;
using Amido.Stacks.Messaging.Azure.ServiceBus.Configuration;
using Amido.Stacks.Messaging.Azure.ServiceBus.Senders;
using Amido.Stacks.Messaging.Azure.ServiceBus.Senders.Routers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Amido.Stacks.Messaging.Azure.ServiceBus.Factories
{
    public class MessageRouterFactory : IMessageRouterFactory
    {
        private readonly IServiceProvider sp;

        public MessageRouterFactory(IServiceProvider serviceProvider)
        {
            this.sp = serviceProvider;
        }

        public IMessageRouter Create(MessageRoutingRouterConfiguration configuration)
        {
            switch (configuration.Strategy?.ToLowerInvariant())
            {
                case "":
                case null:
                case "fallback":
                    return BuildFallbackRouter(configuration);

                // TODO: The following strategies are not implemented yet
                case "roundrobin":
                case "activeactive":
                default:
                    throw new Exception($"The route strategy '{configuration.Strategy}' is not a valid option.");
            }
        }

        public IMessageRouter CreateDefault<T>() where T : IMessageRouter
        {
            if (typeof(T) == typeof(ITopicRouter))
            {
                return sp.GetRequiredService<DefaultMessageRouter<ITopicSender>>();
            }
            else
            {
                return sp.GetRequiredService<DefaultMessageRouter<IQueueSender>>();
            }
        }

        private IMessageRouter BuildFallbackRouter(MessageRoutingRouterConfiguration configuration)
        {
            if (configuration.GetType() == typeof(MessageRoutingTopicRouterConfiguration))
            {
                return new FallbackMessageRouter<ITopicSender>(
                    sp.GetRequiredService<ILogger<FallbackMessageRouter<ITopicSender>>>(),
                    sp.GetRequiredService<IEnumerable<ITopicSender>>(),
                    configuration
                );
            }
            else
            {
                return new FallbackMessageRouter<IQueueSender>(
                    sp.GetRequiredService<ILogger<FallbackMessageRouter<IQueueSender>>>(),
                    sp.GetRequiredService<IEnumerable<IQueueSender>>(),
                    configuration
                );

            }
        }
    }
}
