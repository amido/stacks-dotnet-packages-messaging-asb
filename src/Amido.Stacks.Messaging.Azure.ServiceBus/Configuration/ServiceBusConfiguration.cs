using Amido.Stacks.Configuration;

namespace Amido.Stacks.Messaging.Azure.ServiceBus.Configuration
{
    /// <summary>
    /// General configuration for service bus messaging
    /// </summary>
    public class ServiceBusConfiguration
    {
        /// <summary>
        /// Service Bus Message Sending Configuration
        /// </summary>
        public ServiceBusSenderConfiguration Sender { get; set; }
        public ServiceBusListenerConfiguration Listener { get; set; }
    }

    public class ServiceBusSenderConfiguration
    {
        /// <summary>
        /// Queues to connect to for sending messages(commands)
        /// </summary>
        public ServiceBusQueueConfiguration[] Queues { get; set; }
        /// <summary>
        /// Queues to connect to for sending messages(events)
        /// </summary>
        public ServiceBusTopicConfiguration[] Topics { get; set; }
        /// <summary>
        /// Routing configuration used when multiple queues or topics are used
        /// </summary>
        public MessageRoutingConfiguration Routing { get; set; }
    }

    public abstract class ServiceBusEntityConfiguration
    {
        public string Alias { get; set; }
        /// <summary>
        /// Name of queue/topic
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Connection String used to authenticate on queue/topic
        /// </summary>
        public Secret ConnectionStringSecret { get; set; }
        /// <summary>
        /// Serializer used to serialize/deserialize messages
        /// </summary>
        public string Serializer { get; set; } = Constants.Defaults.CommandSerializer;
    }

    public abstract class ServiceBusSenderEntityConfiguration : ServiceBusEntityConfiguration
    {
        public bool EnableHealthChecks { get; set; } = true;

        //Disabled for now (TBD if we want to enable it, see comment below)
        //public ResilienceConfiguration Resilience { get; set; }
    }

    /* SB SDK provides a default retry strategy
     * TBD if we want to allow users to change it
     * Given it can reduce the resilience when misconfigured
     
    public class ResilienceConfiguration
    {
        /// <summary>
        /// Number of attempts before considering the operation failed
        /// </summary>
        public int Retries { get; set; } = 5;

        /// <summary>
        /// ExceptionHandler used to filter retriable exceptions 
        /// </summary>
        public string ExceptionHandler { get; set; }
    }
    */

    public class ServiceBusQueueConfiguration : ServiceBusSenderEntityConfiguration
    {
    }

    public class ServiceBusTopicConfiguration : ServiceBusQueueConfiguration
    {
        public ServiceBusTopicConfiguration()
        {
            Serializer = Constants.Defaults.EventSerializer;
        }
    }

    public class ServiceBusSubscriptionConfiguration : ServiceBusTopicConfiguration
    {
        public string SubscriptionName { get; set; }
    }

    public class MessageRoutingConfiguration
    {
        public MessageRoutingTopicRouterConfiguration[] Topics { get; set; }
        public MessageRoutingQueueRouterConfiguration[] Queues { get; set; }

    }

    public abstract class MessageRoutingRouterConfiguration
    {
        public string Strategy { get; set; }
        public string[] SendTo { get; set; }
        public string[] TypeFilter { get; set; }
    }

    public class MessageRoutingTopicRouterConfiguration : MessageRoutingRouterConfiguration { }
    public class MessageRoutingQueueRouterConfiguration : MessageRoutingRouterConfiguration { }


    // LISTENER CONFIGURATION

    public class ServiceBusListenerConfiguration
    {
        public ServiceBusQueueListenerConfiguration[] Queues { get; set; }
        public ServiceBusSubscriptionListenerConfiguration[] Topics { get; set; }
    }

    public class ServiceBusQueueListenerConfiguration : ServiceBusEntityConfiguration
    {
        public ServiceBusQueueListenerConfiguration()
        {
            Serializer = Constants.Defaults.CommandSerializer;
        }

        public int ConcurrencyLevel { get; set; } = 1;
        public bool DisableProcessing { get; set; } = false;
        public bool DisableMessageValidation { get; set; }
        public string MessageProcessor { get; set; }
    }

    public class ServiceBusSubscriptionListenerConfiguration : ServiceBusQueueListenerConfiguration
    {
        public ServiceBusSubscriptionListenerConfiguration()
        {
            Serializer = Constants.Defaults.EventSerializer;
        }
        public string SubscriptionName { get; set; }
    }
}
