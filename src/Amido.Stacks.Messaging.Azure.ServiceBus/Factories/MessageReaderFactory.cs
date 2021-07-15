using System;
using System.Collections.Generic;
using System.Linq;
using Amido.Stacks.Messaging.Azure.ServiceBus.Extensions;
using Amido.Stacks.Messaging.Azure.ServiceBus.Serializers;

namespace Amido.Stacks.Messaging.Azure.ServiceBus.Factories
{
    public class MessageReaderFactory : IMessagerReaderFactory
    {
        private readonly IEnumerable<IMessageReader> readers;

        public MessageReaderFactory(IEnumerable<IMessageReader> readers)
        {
            this.readers = readers;
        }

        public IMessageReader CreateReader<T>(string name = null)
        {
            if (name == null)
                throw new Exception("The deserializer name must be provided");

            var reader = readers
                .First(x =>
                    x.GetType().GetTypeNameWithAssembly() == name ||
                    x.GetType().Name == name ||
                    x.GetType().FullName == name);

            return reader as IMessageReader;
        }
    }
}
