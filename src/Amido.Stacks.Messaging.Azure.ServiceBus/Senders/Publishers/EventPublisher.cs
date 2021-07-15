using System.Threading.Tasks;
using Amido.Stacks.Application.CQRS.ApplicationEvents;
using Amido.Stacks.Messaging.Azure.ServiceBus.Senders.Routers;
using Microsoft.Extensions.Logging;

namespace Amido.Stacks.Messaging.Azure.ServiceBus.Senders.Publishers
{
    // TODO: This will become MagicRouterLogicToWithCustomStrategies

    public class EventPublisher : IApplicationEventPublisher
    {
        private readonly ILogger<EventPublisher> _log;
        private readonly ServiceBusAbstractRouter<ITopicRouter> routing;

        public EventPublisher(
            ILogger<EventPublisher> log,
            ServiceBusAbstractRouter<ITopicRouter> routing
        )
        {
            this._log = log;
            this.routing = routing;
        }

        public async Task PublishAsync(IApplicationEvent applicationEvent)
        {
            _log.LogInformation($"Publishing event {applicationEvent.CorrelationId}");

            await routing.RouteAsync(applicationEvent);

            _log.LogInformation($"{applicationEvent.CorrelationId}");
        }
    }
}
