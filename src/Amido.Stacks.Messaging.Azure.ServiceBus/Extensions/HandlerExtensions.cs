using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Amido.Stacks.Messaging.Azure.ServiceBus.Extensions
{
    public static class HandlerExtensions
    {

        /// <summary>
        /// Find Handlers registered in the DI container
        /// Without requiring the user to inject the assembly reference
        /// </summary>
        /// <param name="services"></param>
        /// <param name="handlerTypes"></param>
        /// <returns></returns>
        public static Dictionary<Type, Type> GetRegisteredHandlersFor(
            this IServiceCollection services,
            params Type[] handlerTypes)
        {
            var list = new Dictionary<Type, Type>();
            foreach (var service in services)
            {
                var handler = service.ImplementationType?
                                .GetInterfaces()
                                .FirstOrDefault(x =>
                                    x.IsGenericType &&
                                    handlerTypes.Any(h => h == x.GetGenericTypeDefinition())
                                );

                if (handler != null)
                {
                    list.Add(handler.GenericTypeArguments[0], handler);
                }
            }

            return list;
        }

    }
}
