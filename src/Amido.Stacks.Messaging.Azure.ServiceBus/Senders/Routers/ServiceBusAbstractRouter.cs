using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Amido.Stacks.Messaging.Azure.ServiceBus.Senders.Routers
{
    /// <summary>
    /// An object router that accepts anything to dispatched to a queue or topic using configured routes
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ServiceBusAbstractRouter<T> where T : IMessageRouter
    {
        private IEnumerable<T> routers;

        public ServiceBusAbstractRouter(IEnumerable<T> routers)
        {
            this.routers = routers;
        }

        public async Task RouteAsync(object msg)
        {
            var router = GetRouter(msg.GetType());

            await router.SendAsync(msg);
        }

        private T GetRouter(Type type)
        {
            T router;

            try
            {
                // Note: We could potentially allow first match
                // but will increase the chances of misconfigurations
                // This approach force the developer define each individual route
                // and avoid misconfiguration when using a catch all
                // catch all will only be allowed when a single route is setup (used for fallback and load balancing)
                router = routers.SingleOrDefault(r => r.Match(type));
            }
            catch (InvalidOperationException ex)
                when (ex.Message == "Sequence contains more than one matching element")
            {
                throw new Exception($"The type '{type.FullName}' matches multiple routes. Please make sure the routing contains 'TypeFilter' and the type is not repeated on multiple routes",
                    ex);
            }

            if (router == null)
                throw new Exception($"No router found matching the type '{type.FullName}'");

            return router;
        }
    }
}
