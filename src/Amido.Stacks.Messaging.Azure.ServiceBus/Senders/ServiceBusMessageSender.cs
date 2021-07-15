using System;
using System.Threading;
using System.Threading.Tasks;
using Amido.Stacks.Messaging.Azure.ServiceBus.Configuration;
using Amido.Stacks.Messaging.Azure.ServiceBus.Extensions;
using Amido.Stacks.Messaging.Azure.ServiceBus.Serializers;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Amido.Stacks.Messaging.Azure.ServiceBus.Senders
{
    public class ServiceBusMessageSender : ITopicSender, IQueueSender, IHealthCheck
    {
        private readonly ILogger<ServiceBusMessageSender> log;
        private readonly ISenderClient senderClient;
        private readonly IMessageBuilder messageBuilder;
        private readonly ServiceBusSenderEntityConfiguration config;

        public ServiceBusMessageSender(
            ILogger<ServiceBusMessageSender> log,
            ISenderClient senderClient,
            IMessageBuilder messageBuilder,
            ServiceBusSenderEntityConfiguration config)
        {
            this.log = log;
            this.senderClient = senderClient;
            this.messageBuilder = messageBuilder;
            this.config = config;
            this.Alias = string.IsNullOrEmpty(config.Alias) ? config.Name : config.Alias;
        }

        public string Alias { get; }

        public async Task SendAsync<T>(T item)
        {
            var message = messageBuilder.Build(item);
            log.LogInformation($"Sending item '{message?.GetEnclosedMessageType()}' with MessageId '{message?.MessageId}' to '{senderClient.Path}'.");
            await senderClient.SendAsync(message);
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            if (!config.EnableHealthChecks)
                return HealthCheckResult.Healthy($"The health check for sender client  with alias  '{Alias}' is disabled");

            if (senderClient.IsClosedOrClosing)
                return HealthCheckResult.Degraded($"The sender client  with alias  '{Alias}' is not connected");

            try
            {
                var msgSequenceNumber = await senderClient.ScheduleMessageAsync(new Message(), DateTime.Now.AddSeconds(60));
                await senderClient.CancelScheduledMessageAsync(msgSequenceNumber);
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy($"The sender client with alias  '{Alias}' has failed health check.", ex);
            }

            return HealthCheckResult.Healthy();
        }
    }
}
