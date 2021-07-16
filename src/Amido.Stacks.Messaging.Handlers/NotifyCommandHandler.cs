using System.Threading.Tasks;
using Amido.Stacks.Application.CQRS.Commands;
using Amido.Stacks.Messaging.Commands;
using Amido.Stacks.Messaging.Handlers.TestDependency;

namespace Amido.Stacks.Messaging.Handlers
{
    public class NotifyCommandHandler : ICommandHandler<NotifyCommand, bool>
    {
        private readonly ITestable<NotifyCommand> _testable;

        public NotifyCommandHandler(ITestable<NotifyCommand> testable)
        {
            _testable = testable;
        }

        public Task<bool> HandleAsync(NotifyCommand command)
        {
            _testable.Complete(command);
            return Task.FromResult(true);
        }
    }
}
