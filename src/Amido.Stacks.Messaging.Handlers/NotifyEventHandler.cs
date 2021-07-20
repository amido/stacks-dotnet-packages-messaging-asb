using System.Threading.Tasks;
using Amido.Stacks.Application.CQRS.ApplicationEvents;
using Amido.Stacks.Messaging.Events;
using Amido.Stacks.Messaging.Handlers.TestDependency;

namespace Amido.Stacks.Messaging.Handlers
{
    public class NotifyEventHandler : IApplicationEventHandler<NotifyEvent>
    {
        private readonly ITestable<NotifyEvent> _testable;

        public NotifyEventHandler(ITestable<NotifyEvent> testable)
        {
            _testable = testable;
        }

        public Task HandleAsync(NotifyEvent applicationEvent)
        {
            _testable.Complete(applicationEvent);
            return Task.CompletedTask;
        }
    }
}