using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amido.Stacks.Messaging.Azure.ServiceBus.Listeners;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Amido.Stacks.Messaging.Azure.ServiceBus.Hosts
{
    public class ServiceBusListenerHost : IHostedService
    {
        private readonly ILogger<ServiceBusListenerHost> log;
        private readonly IEnumerable<IMessageListener> listeners;

        public ServiceBusListenerHost(
            ILogger<ServiceBusListenerHost> log,
            IEnumerable<IMessageListener> listeners)
        {
            this.log = log;
            this.listeners = listeners;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            log.LogInformation("Service Bus Listener Host is starting.");

            await Task.WhenAll(listeners.Select(l => l.StartAsync()));

            log.LogInformation("Service Bus Listener Host has starteded.");
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            log.LogInformation("Service Bus Listener Host is stopping.");

            await Task.WhenAll(listeners.Select(l => l.StopAsync()));

            log.LogInformation("Service Bus Listener Host has stopped.");
        }
    }
}
