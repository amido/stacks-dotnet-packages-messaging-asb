using System;
using System.Threading;
using System.Threading.Tasks;
using Amido.Stacks.Application.CQRS.ApplicationEvents;
using Amido.Stacks.Configuration.Extensions;
using Amido.Stacks.Messaging.Azure.ServiceBus.Configuration;
using Amido.Stacks.Messaging.Azure.ServiceBus.Senders;
using Amido.Stacks.Messaging.Events;
using Amido.Stacks.Messaging.Handlers;
using Amido.Stacks.Messaging.Handlers.TestDependency;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using NSubstitute;

namespace Amido.Stacks.Messaging.Azure.ServiceBus.Tests.IntegrationTests.Steps
{
    public class PublishEventFixtures
    {
        private readonly ServiceProvider _provider;
        private readonly ITestable<NotifyEvent> _testable;
        private Guid _correlationId;

        public PublishEventFixtures()
        {
            var assemblies = new[] { typeof(NotifyEvent).Assembly, typeof(NotifyEventHandler).Assembly };
            _testable = Substitute.For<ITestable<NotifyEvent>>();

            var configurationRoot = new ConfigurationBuilder()
                .AddJsonFile("appsettings.integration.topics.json")
                .Build();

            var services = new ServiceCollection()
                .AddLogging()
                .AddSecrets()
                .AddTransient<IApplicationEventHandler<NotifyEvent>, NotifyEventHandler>()
                .AddTransient(_ => _testable)

                .Configure<ServiceBusConfiguration>(configurationRoot.GetSection("ServiceBus"))
                .AddServiceBus()
            ;

            _correlationId = Guid.NewGuid();
            _provider = services.BuildServiceProvider();
        }


        public async Task TheTopicSenderHealthCheckPass()
        {
            var topics = _provider.GetServices<ITopicSender>();
            foreach (var topic in topics)
            {
                var check = await ((IHealthCheck)topic).CheckHealthAsync(null);
                check.Status.Should().Be(HealthStatus.Healthy);
            }
        }

        public void TheCorrectEventIsSentToTheTopic()
        {
            var eventPublisher = _provider.GetService<IApplicationEventPublisher>();
            eventPublisher.PublishAsync(new NotifyEvent(_correlationId, 321, "resourceId")).GetAwaiter().GetResult();
        }

        public void TheHostIsRunning()
        {
            var hostService = _provider.GetService<IHostedService>();
            hostService.StartAsync(CancellationToken.None)
                .GetAwaiter()
                .GetResult();
        }

        public void WaitFor3Seconds()
        {
            int delay = 3;
            Task.Delay(TimeSpan.FromSeconds(delay)).GetAwaiter().GetResult();
        }

        public void TheMessageIsHandledInTheHandler()
        {
            _testable.Received(1)
                .Complete(Arg.Is<NotifyEvent>(applicationEvent => applicationEvent.OperationCode == 321
                                                                  && applicationEvent.CorrelationId == _correlationId));
        }
    }
}
