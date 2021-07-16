using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amido.Stacks.Application.CQRS.Commands;
using Amido.Stacks.Messaging.Azure.ServiceBus.Configuration;
using Amido.Stacks.Messaging.Azure.ServiceBus.Factories;
using Amido.Stacks.Messaging.Azure.ServiceBus.Serializers;
using Amido.Stacks.Messaging.Commands;
using Amido.Stacks.Messaging.Handlers;
using Amido.Stacks.Messaging.Handlers.TestDependency;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace Amido.Stacks.Messaging.Azure.ServiceBus.Tests.UnitTests.Listeners
{
    public class QueueListenerTests
    {
        ServiceBusConfiguration config;
        IServiceCollection services;

        List<IReceiverClient> receiverClients = new List<IReceiverClient>();


        ITestable<NotifyCommand> testable;

        public QueueListenerTests()
        {
            testable = Substitute.For<ITestable<NotifyCommand>>();

            config = new ServiceBusConfiguration()
            {
                Listener = new ServiceBusListenerConfiguration
                {
                    Queues = new[]
                    {
                        new ServiceBusQueueListenerConfiguration
                        {
                            Alias = "c",
                            Name = "CHARLIE"
                        },
                        new ServiceBusQueueListenerConfiguration
                        {
                            Alias = "d",
                            Name = "DELTA"
                        }
                    }
                }
            };

            var receiverFactory = Substitute.For<IMessageReceiverClientFactory>();
            receiverFactory.CreateReceiverClient(Arg.Any<ServiceBusEntityConfiguration>()).Returns((args) =>
            {
                var cfg = (ServiceBusEntityConfiguration)args[0];
                var client = Substitute.For<FakeSBClient>();
                client.Path = cfg.Alias ?? cfg.Name;
                receiverClients.Add(client);
                return client;
            });

            services = new ServiceCollection()
                .AddLogging()
                .AddTransient((b) => Options.Create(config))
                .AddSingleton(receiverFactory)
                .AddTransient(_ => testable)
            ;
        }

        [Theory]
        [InlineData(nameof(JsonMessageSerializer), nameof(NotifyCommand))]
        [InlineData(nameof(JsonMessageSerializer), "")]
        [InlineData(nameof(CloudEventMessageSerializer), "")]
        public async Task Given_A_Message_Is_Received_From_UnknownType_Is_DeadLettered(string serializer, string enclosedTypeName)
        {
            //ARRANGE
            var guid = Guid.NewGuid();

            services
                .AddTransient<ICommandHandler<NotifyCommand, bool>, NotifyCommandHandler>()
                .AddServiceBus()
            ;

            await services.BuildServiceProvider().GetService<IHostedService>().StartAsync(CancellationToken.None);

            Message msg = BuildMessage(serializer, guid);
            msg.SetLockOnMessage();
            msg.UserProperties[MessageProperties.EnclosedMessageType.ToString()] = enclosedTypeName;

            //ACT
            var client = (FakeSBClient)receiverClients.First();
            await client.SendAsyncToReceiver(msg);

            ////ASSERT
            testable.Received(0).Complete(Arg.Any<NotifyCommand>());
            await client.Received(1).DeadLetterAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        }

        [Theory]
        [InlineData(nameof(JsonMessageSerializer))]
        [InlineData(nameof(CloudEventMessageSerializer))]
        public async Task Given_A_Message_Is_Received_From_KnownType_Is_Sent_To_Right_Hanler(string serializer)
        {
            //ARRANGE
            var guid = Guid.NewGuid();

            services
                .AddTransient<ICommandHandler<NotifyCommand, bool>, NotifyCommandHandler>()
                .AddServiceBus()
            ;

            await services.BuildServiceProvider().GetService<IHostedService>().StartAsync(CancellationToken.None);

            Message msg = BuildMessage(serializer, guid);
            msg.SetLockOnMessage();

            //ACT
            var client = (FakeSBClient)receiverClients.First();
            await client.SendAsyncToReceiver(msg);

            ////ASSERT
            testable.Received(1).Complete(Arg.Is<NotifyCommand>(m => m.CorrelationId == guid));
        }


        [Theory]
        [InlineData(nameof(JsonMessageSerializer))]
        [InlineData(nameof(CloudEventMessageSerializer))]
        public async Task Given_A_Message_Is_Received_From_KnownType_NoHandlerAvailable_Should_DeadLetter(string serializer)
        {
            //ARRANGE
            var guid = Guid.NewGuid();

            services.AddServiceBus();

            await services.BuildServiceProvider().GetService<IHostedService>().StartAsync(CancellationToken.None);

            Message msg = BuildMessage(serializer, guid);
            msg.SetLockOnMessage();

            //ACT
            var client = (FakeSBClient)receiverClients.First();
            await client.SendAsyncToReceiver(msg);

            ////ASSERT
            testable.Received(0).Complete(Arg.Any<NotifyCommand>());
            await client.Received(1).DeadLetterAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        }


        private static Message BuildMessage(string serializer, Guid guid)
        {
            var obj = new NotifyCommand(guid, "123");

            if (serializer == nameof(CloudEventMessageSerializer))
                return new CloudEventMessageSerializer().Build<ICommand>(obj);
            else
                return new JsonMessageSerializer().Build<ICommand>(obj);
        }

    }
}
