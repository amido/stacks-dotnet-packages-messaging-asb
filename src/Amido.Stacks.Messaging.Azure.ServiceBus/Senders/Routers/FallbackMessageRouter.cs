using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amido.Stacks.Core.Operations;
using Amido.Stacks.Messaging.Azure.ServiceBus.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Amido.Stacks.Messaging.Azure.ServiceBus.Senders.Routers
{
    public class FallbackMessageRouter<T> : ITopicRouter, IQueueRouter
        where T : IMessageSender
    {
        private readonly ILogger<FallbackMessageRouter<T>> logger;
        private MessageRoutingRouterConfiguration routeConfig;
        private List<T> senders = new List<T>();

        public FallbackMessageRouter(
            ILogger<FallbackMessageRouter<T>> logger,
            IEnumerable<T> senders,
            MessageRoutingRouterConfiguration routeConfig
        )
        {
            this.logger = logger;
            this.routeConfig = routeConfig;

            foreach (var to in routeConfig.SendTo)
            {
                var sender = senders.SingleOrDefault(s => to.Equals(s.Alias, StringComparison.InvariantCultureIgnoreCase));

                if (sender == null)
                    throw new Exception($"Unable to load sender for entity/alias '{to}'. Please make sure the entity name matches to a queue or topic registered in the sender section.");

                this.senders.Add(sender);
            }
        }

        public async Task SendAsync(object message)
        {
            var enumerator = senders.GetEnumerator();
            enumerator.MoveNext();
            while (enumerator.Current != null)
            {
                try
                {
                    await enumerator.Current.SendAsync(message);
                }
                catch (JsonSerializationException ex)
                {
                    logger.LogError(ex, $"Failed to send message {GetMessageIdentifier(message)} to entity '{enumerator.Current.Alias}'");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Failed to send message {GetMessageIdentifier(message)} to entity '{enumerator.Current.Alias}'");

                    if (!enumerator.MoveNext())
                        throw;

                    logger.LogWarning($"Fallback: Sending message {GetMessageIdentifier(message)} to entity '{enumerator.Current.Alias}'");

                    continue;
                }

                break;
            }
        }

        public bool Match(Type type)
        {
            if (routeConfig.TypeFilter == null || routeConfig.TypeFilter.Length == 0)
                return true;

            return routeConfig.TypeFilter.Any(f => f == type.FullName);
        }

        private string GetMessageIdentifier(object message)
        {
            var ctx = message as IOperationContext;

            return ctx == null ? "'<no-CorrelationId>'" : "'{ctx.CorrelationId}'";
        }
    }
}
