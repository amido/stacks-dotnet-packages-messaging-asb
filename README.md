# Amido Stacks Messaging Azure ServiceBus

This library is wrapper around Azure Service Bus.
The main goal is:

    1.) to send a command to a queue or publish an event to a particular topic,
    2.) to listen to an Azure Service Bus queue or a topic as a subscription,
    3.) to parse the message to a predefined strongly typed object when arrives,
    4.) to pass the parsed object to a predefined handler.

## 1. Registration/Usage

### 1.1 Dependencies
 - `Amido.Stacks.Configuration`
 - `Amido.Stacks.Application.CQRS.Abstractions`
 - `Amido.Stacks.DependencyInjection`
 - `Microsoft.Azure.ServiceBus`
 - `Microsoft.Extensions.Hosting`

### 1.2 Currently Supported messages

The library currently supports:
  - sending and receiving commands implementing `Amido.Stacks.Application.CQRS.Commands.ICommands`,
  - publishing and receiving events implementing `Amido.Stacks.Application.CQRS.ApplicationEvents.IApplicationEvent`


### 1.3 Usage in dotnet core application
#### 1.3.1 Command
As an example we are having a `NotifyCommandHandler` as a handler for `NotifyCommand`. The handler implements
`Amido.Stacks.Application.CQRS.Commands.ICommandHandler<NotifyCommand, bool>` and the command implements
`Amido.Stacks.Application.CQRS.Commands.ICommand` interfaces.

***NotifyCommand.cs***
```cs
    public class NotifyCommand : ICommand
    {
        public NotifyCommand(Guid correlationId, string testMember)
        {
            OperationCode = 666;
            CorrelationId = correlationId;
            TestMember = testMember;
        }

        public string TestMember { get; }
        public int OperationCode { get; }
        public Guid CorrelationId { get; }
    }
```

***NotifyCommandHandler.cs***

```cs
    public class NotifyCommandHandler : ICommandHandler<NotifyCommand, bool>
    {
        private readonly ITestable<NotifyCommand> _testable;

        public NotifyCommandHandler(ITestable<NotifyCommand> testable)
        {
            _testable = testable;
        }

        public Task<bool> HandleAsync(NotifyCommand command)
        {
            _testable.Complete(command);
            return Task.FromResult(true);
        }
    }
```

##### 1.3.1.1 CommandDispatcher configuration
The command dispatchers responsibility is to send a command message to a preconfigured queue. The FullName - such as
`Amido.Stacks.Messaging.Commands.NotifyCommand` - of the type (command) is paired
with the queue-name in the ***Routing*** configuration. Each individual queue will have one message sender, therefore the
queue name in the routing - e.g `notifications-command` -  has to match for the name in the routing configuration.
The configuration for the ***CommandDispatcher*** is in the ***ServiceBusSender*** section.

| Queues | Queue Routes | Behaviour |
| --- | --- | --- |
| 1 queue is defined | no routing is defined | sends all messages* |
| 1 queue is defined | 1 routing is defined for one type |  sends only mapped messages**
| 2 or more queues defined| no routing is defined | all commands will fail*** |
| 2 or more queues defined| 1 or more routing is defined | routed messages will be sent |

*defaults all the messages to one queue

**it works as a filter, routed messages are sent, the non routed ones are throwing ***MessageRouteNotDefined*** exception

***routing configuration is needed when more than one queue is defined.

***appsettings.json***

```json
{
    "ServiceBusConfiguration": {
        "Sender": {
            "Queues": [
                {
                    "Name": "notifications-command",
                    "ConnectionStringSecret": {
                        "Identifier": "SERVICEBUS_CONNECTIONSTRING",
                        "Source": "Environment"
                    }
                }
            ],
            "Routes": {
                "QueueRoutes": [
                    {
                        "Name": "notifications-command",
                        "Types": [
                            "Amido.Stacks.Messaging.Commands.NotifyCommand"
                        ]
                    }
                ]
            }
        }
    }
}
```
***Usage***
```Startup.cs
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddTransient<Consumer>();
        services.Configure<ServiceBusSenderConfiguration>(configurationRoot.GetSection("ServiceBusSender"))
        .AddServiceBusCommandDispatcher();
    }
}

public class Consumer
{
    private readonly ICommandDispatcher _dispatcher;

    public Consumer(ICommandDispatcher dispatcher) {
        _dispatcher = dispatcher;
    }

    public async Task SendIt(Data dataToSend)
    {
        // Example usage for an example command
        await _dispatcher.SendAsync(new NotifyClientCommand(... , dataToSend, ...));
    }
}
```
##### 1.3.1.2 Command Listener configuration

The listener can listen to many queues. The ***Name*** describes the name of the queue, ***ConcurrencyLevel***
is the MaxConcurrentCalls, ***DisableProcessing*** is the flag to enable/disable the registration - listening
to Service Bus - and ***DisableMessageValidation*** flag disables/enables the validation on the incoming
messages. The configuration for the ***Listener*** is under ***ServiceBusListener*** section.

***appsettings.json***
```json
{
  "ServiceBusConfiguration": {
        "Listener": {
            "Topics": [
                {
                    "Name": "notifications",
                    "ConcurrencyLevel": 5,
                    "DisableProcessing": false,
                    "ConnectionStringSecret": {
                        "Identifier": "SERVICEBUS_CONNECTIONSTRING",
                        "Source": "Environment"
                    },
                    "DisableMessageValidation": true
                }
            ]
        }
    }
}
```

***Program.cs***

```cs

public static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration(builder =>
        {
            // Add the configuration file
            builder.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true);
        })
        .ConfigureServices((hostContext, services) =>
        {
            services
                .AddLogging()
                .AddSecrets()
                .Configure<ServiceBusConfiguration>(hostContext.Configuration.GetSection("ServiceBusConfiguration"))
                .AddServiceBus()
                .AddTransient<ICommandHandler<NotifyClientCommand, bool>, NotifyClientCommandHandler>();
        });
}
```

#### 1.3.2 Event
In this case the `NotifyEvent` has a `NotifyEventHandler`. The handler implements
`Amido.Stacks.Application.CQRS.ApplicationEvents.IApplicationEventHandler<NotifyCommand, bool>` and the command implements
`Amido.Stacks.Application.CQRS.ApplicationEvents.IApplicationEvent` interfaces.

***NotifyEvent.cs***

```cs
   public class NotifyEvent : IApplicationEvent
    {
        public int OperationCode { get; }
        public Guid CorrelationId { get; }
        public int EventCode { get; }

        public NotifyEvent(int operationCode, Guid correlationId, int eventCode)
        {
            OperationCode = operationCode;
            CorrelationId = correlationId;
            EventCode = eventCode;
        }
    }
```

***NotifyEventHandler.cs***

```cs
     public class NotifyEventHandler : IApplicationEventHandler<NotifyEvent>
     {
         private readonly ITestable<NotifyEvent> _testable;

         public NotifyEventHandler(ITestable<NotifyEvent> testable)
         {
             _testable = testable;
         }

         public Task HandleAsync(NotifyEvent applicationEvent)
         {
            _testable.Complete(applicationEvent);
            return Task.CompletedTask;
         }
     }
```

##### 1.3.1.1 EventPublisher configuration
Its responsibility is to publish an event message to a preconfigured topic. The topic for the event depends on the ***Routing*** configuration.
The following routing table will picture the different configurations:

| Topics | Topic Routes | Behaviour |
| --- | --- | --- |
| 1 topic is defined | no routing is defined | publishes all messages* |
| 1 topic is defined | 1 routing is defined for one type |  publishes only mapped messages**
| 2 or more topics defined| no routing is defined | all events will fail*** |
| 2 or more topics defined| 1 or more routing is defined | routed messages will be published |

*defaults all the messages to one topic

**it works as a filter, routed messages are published, the non routed ones are throwing ***MessageRouteNotDefined*** exception

***routing configuration is needed when more than one topic is defined.

***appsettings.json***

```json
{
    "ServiceBusConfiguration": {
        "ServiceBusSender": {
            "Topics": [
                {
                    "Name": "notification-event",
                    "ConnectionStringSecret": {
                        "Identifier": "SERVICEBUS_CONNECTIONSTRING",
                        "Source": "Environment"
                    }
                }
            ],
            "Routes": {
                "TopicRoutes": [
                    {
                        "Name": "notifications-event",
                        "Types": [
                            "Amido.Stacks.Messaging.Commands.NotifyEvent"
                        ]
                    }
                ]
            }
        }
    }
}
```
***Usage***
```Startup.cs
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddTransient<Consumer>()
            .Configure<ServiceBusSenderConfiguration>(configurationRoot.GetSection("ServiceBusSender"))
            .AddServiceBusEventPublisher();
    }
}

public class Consumer
{
    private readonly IApplicationEventPublisher _eventPublisher;

    public Consumer(IApplicationEventPublisher eventPublisher) {
        _eventPublisher = eventPublisher;
    }

    public async Task PublishIt(Data dataToSend)
    {
        // Example usage for an example command
        await _eventPublisher.PublishAsync(new NotifyEvent(... , dataToSend, ...));
    }
}
```
##### 1.3.1.2 Event Listener configuration

The listener can listen to many topics. The ***Name*** describes the name of the topic, ***ConcurrencyLevel***
is the MaxConcurrentCalls, ***DisableProcessing*** is the flag to enable/disable the registration - listening
to Service Bus - and ***DisableMessageValidation*** flag disables/enables the validation of the incoming
messages. The configuration for the event listener is under ***ServiceBusListener*** section.

***appsettings.json***
```json
{
  "ServiceBusConfiguration": {
        "Listener": {
            "Topics": [
                {
                    "Name": "notifications",
                    "SubscriptionName": "notification-subscription",
                    "ConcurrencyLevel": 5,
                    "DisableProcessing": false,
                    "ConnectionStringSecret": {
                        "Identifier": "SERVICEBUS_CONNECTIONSTRING",
                        "Source": "Environment"
                    },
                    "DisableMessageValidation": true
                }
            ]
        }
    }
}
```
***Program.cs***
```cs
...
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(builder =>
                {
                    // Add the configuration file
                    builder.SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", optional: true);
                })
               .ConfigureServices((hostContext, services) =>
                {

                    services
                        .AddLogging()
                        .AddSecrets()
                        .Configure<ServiceBusConfiguration>(hostContext.Configuration.GetSection("ServiceBusConfiguration"))
                        .AddServiceBus()
                        .AddTransient<IApplicationEventHandler<NotifyEvent>, NotifyEventHandler>();
                });
        }
...
```


### Unrecoverable exceptions
 The unrecoverable exceptions are the exceptions when the parsing of the object fails due to the invalid
 state of the message. We don't want to retry the process of these messages as it would result
 the same exception, therefore they are moved to the dead-letter queue with the specified reason,
 why it has been placed to the dead-letter queue.

 Unrecoverable exceptions are:
  - `Amido.Stacks.Messaging.Azure.ServiceBus.Exceptions.UnrecoverableException` general exception,
  - `Amido.Stacks.Messaging.Azure.ServiceBus.Exceptions.MessageParsingException` parsing related exception

