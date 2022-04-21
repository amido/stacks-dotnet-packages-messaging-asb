using System;
using System.Text;
using Amido.Stacks.Core.Operations;
using Amido.Stacks.Messaging.Azure.ServiceBus.Exceptions;
using Amido.Stacks.Messaging.Azure.ServiceBus.Extensions;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;

namespace Amido.Stacks.Messaging.Azure.ServiceBus.Serializers
{
    public class CloudEventMessageSerializer : IMessageBuilder, IMessageReader
    {
        public Message Build<T>(T body)
        {
            Guid correlationId = GetCorrelationId(body);

            var cloudEvent = new StacksCloudEvent<T>(body, correlationId)
            {
                DataContentType = "application/json",
                Source = GetSource()
            };

            var message = new Message
            {
                CorrelationId = $"{correlationId}",
                ContentType = "application/cloudevents+json;charset=utf-8",
                Body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(cloudEvent))
            };

            return message
                .SetEnclosedMessageType(typeof(StacksCloudEvent<>).MakeGenericType(body.GetType()))
                .SetSerializerType(this.GetType())
                .SetSessionId(body);
        }

        public T Read<T>(Message message)
        {
            Type type = null;

            var messageType = message.GetEnclosedMessageType();

            if (!string.IsNullOrEmpty(messageType))
            {
                type = Type.GetType(messageType);
            }
            else
            {
                throw new MessageSerializationException("The message type in unknown", null);
            }

            // This will make the serializer flexible to deserialize to the type of T when 
            // the message does not contain an Enclosed Message Type
            // The problem with this approach is the serializer won't throw an exception if 
            // the types does not match. 
            //if (type == null)
            //{
            //    type = typeof(T);
            //}

            try
            {
                if (message.Body is null || message.Body.Length == 0)
                {
                    throw new MessageBodyIsNullException($"The body of the message {message.MessageId} is null.", message);
                }

                var body = Encoding.UTF8.GetString(message.Body);
                var obj = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(message.Body), type);

                var cloudEvent = (CloudEvent)obj;
                if (cloudEvent?.SpecVersion != "1.0")
                {
                    throw new MessageSerializationException("Only version 1.0 of the Cloud Events specs is supported.", message);
                }

                return (T)cloudEvent.Data;
            }
            catch (InvalidCastException ex)
            {
                throw new MessageInvalidCastException("Invalid cast of the object in the message body ", message, ex);
            }
            catch (JsonSerializationException ex)
            {
                throw new MessageSerializationException("Failed to deserialize the message ", message, ex);
            }
        }

        private static Guid GetCorrelationId(object body)
        {
            var ctx = body as IOperationContext;
            return ctx?.CorrelationId ?? Guid.NewGuid();
        }

        private Uri GetSource()
        {
            return new Uri($"{Environment.GetEnvironmentVariable("version")}/{Environment.MachineName}", UriKind.Relative);
        }
    }
}