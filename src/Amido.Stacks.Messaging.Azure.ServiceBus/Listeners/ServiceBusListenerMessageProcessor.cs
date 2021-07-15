using System;
using System.Threading;
using System.Threading.Tasks;
using Amido.Stacks.Core.Operations;
using Amido.Stacks.Messaging.Azure.ServiceBus.Configuration;
using Amido.Stacks.Messaging.Azure.ServiceBus.Exceptions;
using Amido.Stacks.Messaging.Azure.ServiceBus.Extensions;
using Amido.Stacks.Messaging.Azure.ServiceBus.Factories;
using Amido.Stacks.Messaging.Azure.ServiceBus.Validators;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;

namespace Amido.Stacks.Messaging.Azure.ServiceBus.Listeners
{
    public class ServiceBusListenerMessageProcessor<T> : IMessageProcessor
    {
        private readonly ServiceBusQueueListenerConfiguration config;
        private readonly ILogger<ServiceBusListenerMessageProcessor<T>> log;
        private readonly IMessagerReaderFactory messageReaderFactory;
        private readonly IMessageHandlerFactory messageHandlerFactory;
        private readonly IValidator<IOperationContext> validator;

        public ServiceBusListenerMessageProcessor(
            ILogger<ServiceBusListenerMessageProcessor<T>> log,
            IMessagerReaderFactory messageReaderFactory,
            IMessageHandlerFactory messageHandlerFactory,
            IValidator<IOperationContext> validator,
            ServiceBusQueueListenerConfiguration config)
        {
            this.validator = validator;
            this.config = config;
            this.log = log;
            this.messageReaderFactory = messageReaderFactory;
            this.messageHandlerFactory = messageHandlerFactory;
        }

        public async Task ProcessAsync(Message message, CancellationToken cancellationToken)
        {
            var messageTypeName = message.GetEnclosedMessageType();
            if (messageTypeName is null)
            {
                throw new MissingEnclosedMessageTypeException(
                    "The EnclosedMessageType is missing from the message header or not a correct valueType",
                    message);
            }

            var serializerName = message.GetSerializerType() ?? config.Serializer;
            if (string.IsNullOrEmpty(serializerName))
            {
                throw new Exception("No serializer has been identified to parse the message");
            }

            var messageReader = messageReaderFactory.CreateReader<T>(serializerName);
            var parsedContent = messageReader.Read<T>(message);

            // TODO: this validation only works for IOperationContext
            if (!config.DisableMessageValidation && parsedContent is IOperationContext)
            {
                validator.Validate(parsedContent as IOperationContext);
            }

            var resolved = messageHandlerFactory.CreateHandlerFor(parsedContent.GetType());
            if (resolved == null)
            {
                throw new MissingHandlerFromIoCException(
                    $"Unable to find a handler for type '{messageTypeName}'",
                    message);
            }

            var fullName = resolved.GetType().FullName;
            log.LogInformation($"Dispatching message '{message.MessageId}' to handler type '{fullName}'.");

            var methodInfo = resolved.GetType().GetMethod("HandleAsync");
            if (methodInfo == null)
            {
                throw new MissingHandlerMethodException(
                    $"Unable to find method 'HandleAsync' on the '{fullName}'",
                    message);
            }

            await ((Task)methodInfo
                .Invoke(resolved, new object[] { parsedContent }))
                .ConfigureAwait(false);

            log.LogInformation($"'{message.MessageId}' has been processed by the '{fullName}'.");
        }
    }
}
