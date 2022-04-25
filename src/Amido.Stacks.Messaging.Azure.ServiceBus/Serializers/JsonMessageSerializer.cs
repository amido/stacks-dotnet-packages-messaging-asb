using System;
using System.Text;
using Amido.Stacks.Core.Operations;
using Amido.Stacks.Messaging.Azure.ServiceBus.Exceptions;
using Amido.Stacks.Messaging.Azure.ServiceBus.Extensions;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;

namespace Amido.Stacks.Messaging.Azure.ServiceBus.Serializers
{
    public class JsonMessageSerializer : IMessageReader, IMessageBuilder
    {
        public Message Build<T>(T body)
        {
            Guid correlationId = GetCorrelationId(body);

            var convertedMessage = new Message
            {
                CorrelationId = $"{correlationId}",
                ContentType = "application/json;charset=utf-8",
                Body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(body))
            };

            return convertedMessage
                .SetEnclosedMessageType(body.GetType())
                .SetSerializerType(GetType())
                .SetSessionId(body);
        }

        /// <summary>
        /// Read the message body and deserialize into the type T
        /// In case the message contains the type enclosed, the serializer will first 
        /// deserialize to the type provided in the message then 
        /// cast the result to the type of T.
        /// This operation will throw an exception if the enclosed message type is not convertible to the type of T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        /// <returns></returns>
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

                return (T)JsonConvert.DeserializeObject(Encoding.UTF8.GetString(message.Body), type);
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

    }
}