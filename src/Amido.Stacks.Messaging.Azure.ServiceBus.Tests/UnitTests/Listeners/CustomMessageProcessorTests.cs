using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amido.Stacks.Application.CQRS.ApplicationEvents;
using Amido.Stacks.Messaging.Azure.ServiceBus.Configuration;
using Amido.Stacks.Messaging.Azure.ServiceBus.Factories;
using Amido.Stacks.Messaging.Azure.ServiceBus.Listeners;
using Amido.Stacks.Messaging.Azure.ServiceBus.Serializers;
using Amido.Stacks.Messaging.Events;
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
    public class CustomMessageProcessorTests
    {
        ServiceBusConfiguration config;
        IServiceCollection services;

        List<IReceiverClient> receiverClients = new List<IReceiverClient>();


        ITestable<NotifyEvent> testable;

        public CustomMessageProcessorTests()
        {
            testable = Substitute.For<ITestable<NotifyEvent>>();

            config = new ServiceBusConfiguration()
            {
                Listener = new ServiceBusListenerConfiguration
                {
                    Topics = new[]
                    {
                        new ServiceBusSubscriptionListenerConfiguration
                        {
                            Alias = "a",
                            Name = "ALPHA",
                            MessageProcessor = typeof(CustomMessageProcessor).FullName
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
        [InlineData(nameof(JsonMessageSerializer))]
        [InlineData(nameof(CloudEventMessageSerializer))]
        public async Task Given_A_Message_Is_Received_From_Topic_Is_Processed_By_CustomMessageProcessor_And_Completed(string serializer)
        {
            //ARRANGE
            var guid = Guid.NewGuid();

            services
                .AddTransient<IMessageProcessor, CustomMessageProcessor>()
                .AddServiceBus()
            ;

            await services.BuildServiceProvider().GetService<IHostedService>().StartAsync(CancellationToken.None);

            Message msg = BuildMessage(serializer, guid);
            var lockToken = msg.SetLockOnMessage();

            //ACT
            var client = (FakeSBClient)receiverClients.First();
            await client.SendAsyncToReceiver(msg);

            ////ASSERT
            testable.Received(1).Complete(Arg.Is<NotifyEvent>(e => e.CorrelationId == guid));
            await client.Received(1).CompleteAsync(Arg.Is<string>(a => a == lockToken.ToString()));
            await client.Received(0).DeadLetterAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        }


        [Fact]
        public async Task Given_A_Message_Is_Received_From_Topic_Is_Failed_By_CustomMessageProcessor_ShouldRetry()
        {
            //ARRANGE
            var guid = Guid.NewGuid();

            services
                .AddTransient<IMessageProcessor, CustomMessageProcessor>()
                .AddServiceBus()
            ;

            await services.BuildServiceProvider().GetService<IHostedService>().StartAsync(CancellationToken.None);

            Message msg = BuildMessage(null, guid);
            msg.SetLockOnMessage();

            //Simulate Failure
            testable.When(c => c.Complete(Arg.Any<NotifyEvent>())).Do(X => { throw new Exception(); });

            //ACT
            var client = (FakeSBClient)receiverClients.First();
            await client.SendAsyncToReceiver(msg);

            ////ASSERT
            testable.Received(1).Complete(Arg.Is<NotifyEvent>(e => e.CorrelationId == guid));
            await client.Received(0).CompleteAsync(Arg.Any<string>());
            await client.Received(0).DeadLetterAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task Given_A_Topic_With_Invalid_CustomMessageProcessor_ShouldFail_OnStartup()
        {
            config.Listener.Topics[0].MessageProcessor = "asd";

            services.AddServiceBus();

            await Assert.ThrowsAsync<Exception>(async () => await services.BuildServiceProvider().GetService<IHostedService>().StartAsync(CancellationToken.None));
        }

        private static Message BuildMessage(string serializer, Guid guid)
        {
            var obj = new NotifyEvent(guid, 123);

            if (serializer == nameof(CloudEventMessageSerializer))
                return new CloudEventMessageSerializer().Build<IApplicationEvent>(obj);
            else
                return new JsonMessageSerializer().Build<IApplicationEvent>(obj);
        }
    }
}
