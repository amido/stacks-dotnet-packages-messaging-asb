using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amido.Stacks.Application.CQRS.ApplicationEvents;
using Amido.Stacks.Application.CQRS.Commands;
using Amido.Stacks.Configuration.Extensions;
using Amido.Stacks.Messaging.Azure.ServiceBus.Configuration;
using Amido.Stacks.Messaging.Azure.ServiceBus.Factories;
using Amido.Stacks.Messaging.Commands;
using Amido.Stacks.Messaging.Events;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace Amido.Stacks.Messaging.Azure.ServiceBus.Tests.UnitTests.Routers
{
    public class FallbackMessageRouterTests
    {
        ServiceBusConfiguration config;
        IServiceCollection services;

        List<ISenderClient> senderClients = new List<ISenderClient>();

        public FallbackMessageRouterTests()
        {
            config = new ServiceBusConfiguration()
            {
                Sender = new ServiceBusSenderConfiguration
                {
                    Topics = new[]
                    {
                        new ServiceBusTopicConfiguration
                        {
                            Alias = "a",
                            Name = "ALPHA"
                        },
                        new ServiceBusTopicConfiguration
                        {
                            Alias = "b",
                            Name = "BETA"
                        }
                    },
                    Queues = new[]
                    {
                        new ServiceBusQueueConfiguration
                        {
                            Alias = "c",
                            Name = "CHARLIE"
                        },
                        new ServiceBusQueueConfiguration
                        {
                            Alias = "d",
                            Name = "DELTA"
                        }
                    },
                    Routing = new MessageRoutingConfiguration
                    {
                        Topics = new[]
                        {
                            new MessageRoutingTopicRouterConfiguration
                            {
                                Strategy = "fallback",
                                SendTo = new [] {  "b", "a", "b" }
                            },
                        },
                        Queues = new[]
                        {
                            new MessageRoutingQueueRouterConfiguration
                            {
                                SendTo = new [] { "c", "d", "c" }
                            },
                        },
                    }
                }
            };

            var senderFactory = Substitute.For<IMessageSenderClientFactory>();
            senderFactory.CreateSenderClient(Arg.Any<ServiceBusEntityConfiguration>()).Returns((args) =>
            {
                var cfg = (ServiceBusEntityConfiguration)args[0];
                var client = Substitute.For<FakeSBClient>();
                client.Path = cfg.Alias ?? cfg.Name;
                senderClients.Add((ISenderClient)client);
                return client;
            });

            services = new ServiceCollection()
                .AddLogging()
                .AddTransient<IOptions<ServiceBusConfiguration>>((b) => Options.Create<ServiceBusConfiguration>(config))
                .AddSecrets()
                .AddSingleton<IMessageSenderClientFactory>(senderFactory)
            ;
        }


        [Fact]
        public async Task Given_MultipleQueues_MultipleRoutesWithFilters_Commands_Are_Routed_To_Correct_Queue()
        {
            Assert.True(config.Sender.Queues.Length > 1);
            Assert.True(config.Sender.Routing.Queues.Length == 1);

            //ARRANGE
            var guid = Guid.NewGuid();
            var route = config.Sender.Routing.Queues.First();
            services.AddServiceBus();

            senderClients.Single(s => s.Path == route.SendTo.First()).SendAsync(Arg.Any<Message>()).Returns(Task.FromException(new Exception("failed")));

            //ACT
            var dispatcher = services.BuildServiceProvider().GetRequiredService<ICommandDispatcher>();
            await dispatcher.SendAsync(new NotifyCommand(guid, "123"));


            //ASSERT
            await senderClients.Single(s => s.Path == route.SendTo.First())
                .Received(1).SendAsync(Arg.Is<Message>(m => m.CorrelationId == guid.ToString()));

            await senderClients.Single(s => s.Path == route.SendTo.Skip(1).First())
                .Received(1).SendAsync(Arg.Is<Message>(m => m.CorrelationId == guid.ToString()));
        }

        [Fact]
        public async Task Given_MultipleTopics_SingleFallbackRoutes_Events_Are_SendTo_Primary_Then_Secondary()
        {
            Assert.True(config.Sender.Topics.Length > 1);
            Assert.True(config.Sender.Routing.Topics.Length == 1);

            //ARRANGE
            var guid = Guid.NewGuid();
            var route = config.Sender.Routing.Topics.First();
            services.AddServiceBus();

            senderClients.Single(s => s.Path == route.SendTo.First()).SendAsync(Arg.Any<Message>()).Returns(Task.FromException(new Exception("failed")));

            //ACT
            var publisher = services.BuildServiceProvider().GetRequiredService<IApplicationEventPublisher>();
            await publisher.PublishAsync(new NotifyEvent(guid, 1));

            //ASSERT
            await senderClients.Single(s => s.Path == route.SendTo.First())
                .Received(1).SendAsync(Arg.Is<Message>(m => m.CorrelationId == guid.ToString()));

            await senderClients.Single(s => s.Path == route.SendTo.Skip(1).First())
                .Received(1).SendAsync(Arg.Is<Message>(m => m.CorrelationId == guid.ToString()));
        }

    }
}
