using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amido.Stacks.Application.CQRS.ApplicationEvents;
using Amido.Stacks.Application.CQRS.Commands;
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
    public class DefaultMessageRouterTests
    {
        ServiceBusConfiguration config;
        IServiceCollection services;

        List<ISenderClient> senderClients = new List<ISenderClient>();

        public DefaultMessageRouterTests()
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
                                SendTo = new [] {  "b" }
                            },
                            new MessageRoutingTopicRouterConfiguration
                            {
                                SendTo = new [] {  "a" }
                            }
                        },
                        Queues = new[]
                        {
                            new MessageRoutingQueueRouterConfiguration
                            {
                                SendTo = new [] { "d" }
                            },
                            new MessageRoutingQueueRouterConfiguration
                            {
                                SendTo = new [] { "c" }
                            }
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
                .AddSingleton<IMessageSenderClientFactory>(senderFactory)
            ;
        }

        [Fact]
        public void Given_MultipleQueues_NoRouting_AddServiceBus_ThrowsAnException()
        {
            config.Sender.Routing = null;
            config.Sender.Topics = null;

            Assert.True(config.Sender.Queues.Length > 1);
            Assert.Throws<Exception>(services.AddServiceBus);
        }

        [Fact]
        public void Given_MultipleTopics_NoRouting_AddServiceBus_ThrowsAnException()
        {
            config.Sender.Routing = null;
            config.Sender.Queues = null;

            Assert.True(config.Sender.Topics.Length > 1);
            Assert.Throws<Exception>(services.AddServiceBus);
        }

        [Fact]
        public async Task GivenA_SingleQueue_NoRouting_All_Commands_Are_Routed_Equaly()
        {
            var guid = Guid.NewGuid();

            config.Sender.Routing = null;
            config.Sender.Topics = null;
            config.Sender.Queues = config.Sender.Queues.Take(1).ToArray();
            services.AddServiceBus();

            var commandDispatcher = services.BuildServiceProvider().GetRequiredService<ICommandDispatcher>();
            await commandDispatcher.SendAsync(new NotifyCommand(guid, "asdasd"));

            await senderClients.First().Received(1).SendAsync(Arg.Is<Message>(m => m.CorrelationId == guid.ToString()));

            await commandDispatcher.SendAsync(new NotifyCommandWithAnnotation(guid, "asdasd"));
            await commandDispatcher.SendAsync(new NotifyClientCommandWithoutHandler());

            senderClients.First().Received(3);
        }

        [Fact]
        public async Task GivenA_SingleTopic_NoRouting_All_Events_Are_Routed_Equaly()
        {
            var guid = Guid.NewGuid();

            config.Sender.Routing = null;
            config.Sender.Queues = null;
            config.Sender.Topics = config.Sender.Topics.Take(1).ToArray();
            services.AddServiceBus();

            var eventPublisher = services.BuildServiceProvider().GetRequiredService<IApplicationEventPublisher>();
            await eventPublisher.PublishAsync(new NotifyEvent(guid, 2));

            await senderClients.First().Received(1).SendAsync(Arg.Is<Message>(m => m.CorrelationId == guid.ToString()));

            await eventPublisher.PublishAsync(new DummyEvent());
            senderClients.First().Received(2);
        }

        [Fact]
        public async Task Given_MultipleQueues_SingleRoute_All_Commands_Are_Routed_Equaly()
        {
            var guid = Guid.NewGuid();

            var route = config.Sender.Routing.Queues.First();
            config.Sender.Routing.Queues = new[] { route };
            services.AddServiceBus();

            var commandDispatcher = services.BuildServiceProvider().GetRequiredService<ICommandDispatcher>();
            await commandDispatcher.SendAsync(new NotifyCommand(guid, "asdasd"));


            Assert.True(config.Sender.Queues.Length > 1);
            Assert.True(config.Sender.Routing.Queues.Length == 1);

            await senderClients.Single(s => s.Path == route.SendTo.First())
                .Received(1).SendAsync(Arg.Is<Message>(m => m.CorrelationId == guid.ToString()));

            await senderClients.First(s => s.Path != route.SendTo.First())
                .DidNotReceive().SendAsync(Arg.Any<Message>());
        }

        [Fact]
        public async Task Given_MultipleTopics_SingleRoute_All_Events_Are_Routed_Equaly()
        {
            var guid = Guid.NewGuid();

            var route = config.Sender.Routing.Topics.First();
            config.Sender.Routing.Topics = new[] { route };
            services.AddServiceBus();

            var publisher = services.BuildServiceProvider().GetRequiredService<IApplicationEventPublisher>();
            await publisher.PublishAsync(new NotifyEvent(guid, 1));

            Assert.True(config.Sender.Topics.Length > 1);
            Assert.True(config.Sender.Routing.Topics.Length == 1);

            await senderClients.Single(s => s.Path == route.SendTo.First())
                .Received(1).SendAsync(Arg.Is<Message>(m => m.CorrelationId == guid.ToString()));

            await senderClients.First(s => s.Path != route.SendTo.First())
                .DidNotReceive().SendAsync(Arg.Any<Message>());
        }

        [Fact]
        public async Task Given_MultipleTopics_MultipleRoutesWithoutTypeFilters_ThrowsException()
        {
            // NOTE: only a single route should match a type

            Assert.True(config.Sender.Topics.Length > 1);
            Assert.True(config.Sender.Routing.Topics.Length > 1);

            //ARRANGE
            var guid = Guid.NewGuid();

            var route1 = config.Sender.Routing.Topics.First();
            route1.TypeFilter = null;

            var route2 = config.Sender.Routing.Topics.Skip(1).First();
            route2.TypeFilter = null;

            services.AddServiceBus();

            //ACT
            var publisher = services.BuildServiceProvider().GetRequiredService<IApplicationEventPublisher>();

            await Assert.ThrowsAsync<Exception>(() => publisher.PublishAsync(new NotifyEvent(guid, 1)));
        }

        [Fact]
        public async Task Given_MultipleQueues_MultipleRoutesWithFilters_Commands_Are_Routed_To_Correct_Queue()
        {
            Assert.True(config.Sender.Queues.Length > 1);
            Assert.True(config.Sender.Routing.Queues.Length > 1);

            //ARRANGE
            var guid1 = Guid.NewGuid();
            var route1 = config.Sender.Routing.Queues.First();
            route1.TypeFilter = new[] { typeof(NotifyCommand).FullName };

            var guid2 = Guid.NewGuid();
            var route2 = config.Sender.Routing.Queues.Skip(1).First();
            route2.TypeFilter = new[] { typeof(NotifyCommandWithAnnotation).FullName };

            services.AddServiceBus();

            //ACT
            var dispatcher = services.BuildServiceProvider().GetRequiredService<ICommandDispatcher>();
            await dispatcher.SendAsync(new NotifyCommand(guid1, "123"));
            await dispatcher.SendAsync(new NotifyCommandWithAnnotation(guid2, "abc"));

            //ASSERT
            await senderClients.Single(s => s.Path == route1.SendTo.First())
                .Received(1).SendAsync(Arg.Is<Message>(m => m.CorrelationId == guid1.ToString()));

            await senderClients.Single(s => s.Path == route2.SendTo.First())
                .Received(1).SendAsync(Arg.Is<Message>(m => m.CorrelationId == guid2.ToString()));
        }

        [Fact]
        public async Task Given_MultipleTopics_MultipleRoutesWithFilters_Events_Are_Routed_To_Correct_Queue()
        {
            Assert.True(config.Sender.Topics.Length > 1);
            Assert.True(config.Sender.Routing.Topics.Length > 1);

            //ARRANGE
            var guid1 = Guid.NewGuid();
            var route1 = config.Sender.Routing.Topics.First();
            route1.TypeFilter = new[] { typeof(NotifyEvent).FullName };

            var guid2 = Guid.NewGuid();
            var route2 = config.Sender.Routing.Topics.Skip(1).First();
            route2.TypeFilter = new[] { typeof(DummyEvent).FullName };

            services.AddServiceBus();

            //ACT
            var publisher = services.BuildServiceProvider().GetRequiredService<IApplicationEventPublisher>();
            await publisher.PublishAsync(new NotifyEvent(guid1, 1));
            await publisher.PublishAsync(new DummyEvent(guid2));

            //ASSERT
            await senderClients.Single(s => s.Path == route1.SendTo.First())
                .Received(1).SendAsync(Arg.Is<Message>(m => m.CorrelationId == guid1.ToString()));

            await senderClients.Single(s => s.Path == route2.SendTo.First())
                .Received(1).SendAsync(Arg.Is<Message>(m => m.CorrelationId == guid2.ToString()));
        }

    }
}
