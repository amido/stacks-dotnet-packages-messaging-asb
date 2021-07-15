using System;
using Newtonsoft.Json;

namespace Amido.Stacks.Messaging.Azure.ServiceBus.Serializers
{
    /// <summary>
    /// Represents a CloudEvent
    /// </summary>
    public class CloudEvent
    {
        public const string MediaType = "application/cloudevents";

        /// <summary>
        /// CloudEvent 'datacontenttype' attribute. Content type of the 'data' attribute value.
        /// This attribute enables the data attribute to carry any type of content, whereby
        /// format and encoding might differ from that of the chosen event format.
        /// </summary>
        /// <see cref="https://github.com/cloudevents/spec/blob/master/spec.md#contenttype"/>
        [JsonProperty("datacontenttype")] public string DataContentType { get; set; }

        /// <summary>
        /// CloudEvent 'data' content.  The event payload. The payload depends on the type
        /// and the 'schemaurl'. It is encoded into a media format which is specified by the
        /// 'contenttype' attribute (e.g. application/json).
        /// </summary>
        /// <see cref="https://github.com/cloudevents/spec/blob/master/spec.md#data-1"/>
        [JsonProperty("data")] public object Data { get; set; }

        /// <summary>
        /// CloudEvent 'id' attribute. ID of the event. The semantics of this string are explicitly
        /// undefined to ease the implementation of producers. Enables deduplication.
        /// </summary>
        /// <see cref="https://github.com/cloudevents/spec/blob/master/spec.md#id"/>
        [JsonProperty("id")] public string Id { get; set; }

        /// <summary>
        /// CloudEvents 'dataschema' attribute. A link to the schema that the data attribute
        /// adheres to. Incompatible changes to the schema SHOULD be reflected by a
        /// different URI.
        /// </summary>
        /// <see cref="https://github.com/cloudevents/spec/blob/master/spec.md#dataschema"/>
        [JsonProperty("dataschema")] public Uri DataSchema { get; set; }

        /// <summary>
        /// CloudEvents 'source' attribute. This describes the event producer. Often this
        /// will include information such as the type of the event source, the
        /// organization publishing the event, the process that produced the
        /// event, and some unique identifiers.
        /// </summary>
        /// <see cref="https://github.com/cloudevents/spec/blob/master/spec.md#source"/>
        [JsonProperty("source")] public Uri Source { get; set; }

        /// <summary>
        /// CloudEvents 'specversion' attribute. The version of the CloudEvents
        /// specification which the event uses. This enables the interpretation of the context.
        /// </summary>
        /// <see cref="https://github.com/cloudevents/spec/blob/master/spec.md#specversion"/>
        [JsonProperty("specversion")] public string SpecVersion { get; set; }

        /// <summary>
        /// CloudEvents 'subject' attribute. This describes the subject of the event in the context
        /// of the event producer (identified by source). In publish-subscribe scenarios, a subscriber
        /// will typically subscribe to events emitted by a source, but the source identifier alone
        /// might not be sufficient as a qualifier for any specific event if the source context has
        /// internal sub-structure.
        /// </summary>
        /// <see cref="https://github.com/cloudevents/spec/blob/master/spec.md#subject"/>
        [JsonProperty("subject")] public string Subject { get; set; }

        /// <summary>
        /// CloudEvents 'time' attribute. Timestamp of when the event happened.
        /// </summary>
        /// <see cref="https://github.com/cloudevents/spec/blob/master/spec.md#time"/>
        [JsonProperty("time")] public DateTime? Time { get; set; }

        /// <summary>
        /// CloudEvents 'type' attribute. Type of occurrence which has happened.
        /// Often this attribute is used for routing, observability, policy enforcement, etc.
        /// </summary>
        /// <see cref="https://github.com/cloudevents/spec/blob/master/spec.md#type"/>
        [JsonProperty("type")] public string Type { get; set; }
    }
}