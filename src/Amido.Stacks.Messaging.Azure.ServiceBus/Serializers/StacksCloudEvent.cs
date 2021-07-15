using System;
using Newtonsoft.Json;

namespace Amido.Stacks.Messaging.Azure.ServiceBus.Serializers
{
    public class StacksCloudEvent<T> : CloudEvent
    {
        internal StacksCloudEvent()
        {
            SpecVersion = "1.0";
            Time = DateTime.UtcNow;
        }

        internal StacksCloudEvent(T data, Guid correlationId)
        {
            Data = data;
            Type = data.GetType().FullName;
            CorrelationId = correlationId;

            SpecVersion = "1.0";

            var ce = data as ICloudEvent;

            Id = ce?.Id ?? Guid.NewGuid().ToString();
            Subject = ce?.Subject;
            Time = ce?.Time ?? DateTime.UtcNow;

        }

        [JsonProperty("correlationid")] public Guid CorrelationId { get; set; }

        [JsonProperty("data")]
        public new T Data
        {
            get => (T)base.Data;
            set => base.Data = value;
        }
    }
}