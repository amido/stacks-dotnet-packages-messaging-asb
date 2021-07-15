using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amido.Stacks.Messaging.Azure.ServiceBus.Configuration;
using Amido.Stacks.Messaging.Azure.ServiceBus.Exceptions;
using Amido.Stacks.Messaging.Azure.ServiceBus.Extensions;
using Amido.Stacks.Messaging.Azure.ServiceBus.Factories;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.Logging;

namespace Amido.Stacks.Messaging.Azure.ServiceBus.Listeners
{
    public class ServiceBusListener : IMessageListener
    {
        private readonly ILogger<ServiceBusListener> log;
        private readonly IMessageReceiverClientFactory messageReceiverClientFactory;
        private readonly IMessageProcessorFactory messageProcessorFactory;
        private readonly ServiceBusQueueListenerConfiguration configuration;
        private IMessageProcessor messageProcessor;
        private IReceiverClient queueClient;

        public ServiceBusListener(
            ILogger<ServiceBusListener> log,
            IMessageReceiverClientFactory messageReceiverClientFactory,
            IMessageProcessorFactory messageProcessorFactory,
            ServiceBusQueueListenerConfiguration configuration)
        {
            this.log = log;
            this.messageReceiverClientFactory = messageReceiverClientFactory;
            this.messageProcessorFactory = messageProcessorFactory;
            this.configuration = configuration;
        }

        public async Task StartAsync()
        {
            log.LogInformation(
                $"Starting Listener for entity '{configuration.Name}' with ConcurrencyLevel of {configuration.ConcurrencyLevel}.");

            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                // Maximum number of concurrent calls to the callback ProcessMessagesAsync(), set to 1 for simplicity.
                // Set it according to how many messages the application wants to process in parallel.
                MaxConcurrentCalls = configuration.ConcurrencyLevel,

                // Indicates whether the message pump should automatically complete the messages after returning from user callback.
                // False below indicates the complete operation is handled by the user callback as in ProcessMessagesAsync().
                // We disable autocomplete to avoid releasing the message on failure, waiting the lock timeout to do the job
                AutoComplete = false
            };

            if (configuration.DisableProcessing)
            {
                log.LogInformation($"Listener won't start for entity '{configuration.Name}' because the processing is disabled.");
                return;
            }

            // Start the function that processes messages.
            this.messageProcessor = messageProcessorFactory.Create(configuration);
            this.queueClient = await messageReceiverClientFactory.CreateReceiverClient(configuration);
            this.queueClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);

            log.LogInformation($"Listener started for entity '{configuration.Name}' because the processing is disabled.");
        }

        public async Task StopAsync()
        {
            log.LogInformation($"Stopping Queue Listener for entity '{configuration.Name}'.");

            if (queueClient?.IsClosedOrClosing != false)
            {
                log.LogInformation($"Listener was already Stopped for entity '{configuration.Name}'.");
            }

            await queueClient.CloseAsync();
            log.LogInformation($"Listener Stopped for entity '{configuration.Name}'.");
        }

        private async Task ProcessMessagesAsync(Message message, CancellationToken token)
        {
            log.LogInformation($"Message received from queue '{configuration.Name}'.");
            using (log.BeginScope(new Dictionary<string, object>
            {
                [nameof(message.CorrelationId)] = message.CorrelationId,
                [nameof(message.MessageId)] = message.MessageId,
                [nameof(MessageProperties.EnclosedMessageType)] = message.GetEnclosedMessageType()
            }))
            {
                try
                {
                    log.LogInformation($"Handling message '{message.MessageId}' from queue '{configuration.Name}'.");

                    await messageProcessor.ProcessAsync(message, token);

                    await queueClient.CompleteAsync(message.SystemProperties.LockToken);

                    log.LogInformation(
                        $"Successfully handled message '{message.MessageId}' from queue '{configuration.Name}'.");
                }
                catch (UnrecoverableException ex)
                {
                    log.LogError(ex,
                        $"Failed to handle message '{message.MessageId}' from queue '{configuration.Name}'.");

                    await queueClient.DeadLetterAsync(message.SystemProperties.IsLockTokenSet ? message.SystemProperties.LockToken : string.Empty,
                        ex.Message ?? "The parsing of the object is invalid");
                }
                catch (Exception ex)
                {
                    log.LogError(ex,
                        $"Failed to handled message '{message.MessageId}' from queue '{configuration.Name}'.");

                    //Throw is not required, message will be redelivered when lock times out
                }
            }
        }

        private Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            log.LogError(exceptionReceivedEventArgs.Exception,
                $"Message queue operation failed with message '{exceptionReceivedEventArgs.Exception.Message}'.");

            return Task.CompletedTask;
        }
    }
}
