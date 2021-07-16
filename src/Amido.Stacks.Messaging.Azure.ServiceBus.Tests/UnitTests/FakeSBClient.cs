using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;

namespace Amido.Stacks.Messaging.Azure.ServiceBus.Tests.UnitTests
{
    /// <summary>
    /// Test implementation used to mock/fake behaviours of ServiceBusClient(Sender/Receiver)
    /// </summary>
    public class FakeSBClient : ISenderClient, IReceiverClient
    {
        public string ClientId { get; set; }
        public bool IsClosedOrClosing { get; set; }
        public string Path { get; set; }
        public TimeSpan OperationTimeout { get; set; }
        public ServiceBusConnection ServiceBusConnection { get; set; }
        public bool OwnsConnection { get; set; }
        public IList<ServiceBusPlugin> RegisteredPlugins { get; set; } = new List<ServiceBusPlugin>();
        public int PrefetchCount { get; set; }
        public ReceiveMode ReceiveMode { get; set; }
        public virtual Task AbandonAsync(string lockToken, IDictionary<string, object> propertiesToModify = null) { return Task.CompletedTask; }
        public virtual Task CancelScheduledMessageAsync(long sequenceNumber) { return Task.CompletedTask; }
        public virtual Task CloseAsync() { return Task.CompletedTask; }
        public virtual Task CompleteAsync(string lockToken) { return Task.CompletedTask; }
        public virtual Task DeadLetterAsync(string lockToken, IDictionary<string, object> propertiesToModify = null) { return Task.CompletedTask; }
        public virtual Task DeadLetterAsync(string lockToken, string deadLetterReason, string deadLetterErrorDescription = null) { return Task.CompletedTask; }
        public void RegisterMessageHandler(Func<Message, CancellationToken, Task> handler, Func<ExceptionReceivedEventArgs, Task> exceptionReceivedHandler) { internalHandler = handler; }
        public void RegisterMessageHandler(Func<Message, CancellationToken, Task> handler, MessageHandlerOptions messageHandlerOptions) { this.RegisterMessageHandler(handler, messageHandlerOptions.ExceptionReceivedHandler); }
        public virtual void RegisterPlugin(ServiceBusPlugin serviceBusPlugin) { }
        public virtual Task<long> ScheduleMessageAsync(Message message, DateTimeOffset scheduleEnqueueTimeUtc) { return Task.FromResult(0L); }
        public virtual Task SendAsync(Message message) { return Task.CompletedTask; } //This is being handled by NSubstitute for call check
        public async Task SendAsyncToReceiver(Message message)
        {
            if (message == null)
            {
                internalExceptionReceivedHandler?.Invoke(new ExceptionReceivedEventArgs(new Exception("Error handlong the message"), "", "", "", ""));
            }

            if (internalHandler != null)
                await internalHandler.Invoke(message, CancellationToken.None);
        }
        public async Task SendAsync(IList<Message> messageList)
        {
            foreach (var msg in messageList)
            {
                await this.SendAsync(msg);
            }
        }
        public virtual void UnregisterPlugin(string serviceBusPluginName) { }

        public Task UnregisterMessageHandlerAsync(TimeSpan inflightMessageHandlerTasksWaitTimeout)
        {
            throw new NotImplementedException();
        }

        public Func<Message, CancellationToken, Task> internalHandler { get; set; }
        public Func<ExceptionReceivedEventArgs, Task> internalExceptionReceivedHandler { get; set; }
    }
}
