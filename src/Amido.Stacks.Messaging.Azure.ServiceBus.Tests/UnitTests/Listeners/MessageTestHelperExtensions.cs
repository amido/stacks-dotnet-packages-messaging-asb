using System;
using System.Reflection;
using Microsoft.Azure.ServiceBus;

namespace Amido.Stacks.Messaging.Azure.ServiceBus.Tests.UnitTests.Listeners
{
    public static class MessageTestHelperExtensions
    {
        public static Guid SetLockOnMessage(this Message message)
        {
            var lockToken = Guid.NewGuid();

            var systemProperties = message.SystemProperties;
            var type = systemProperties.GetType();
            type.GetMethod("set_LockTokenGuid", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(systemProperties, new object[] { lockToken });
            type.GetMethod("set_SequenceNumber", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(systemProperties, new object[] { 0 });

            return lockToken;
        }
    }
}
