using System;
using Amido.Stacks.Application.CQRS.Commands;

namespace Amido.Stacks.Messaging.Commands
{
    public class NotifyClientCommandWithoutHandler : ICommand
    {
        public int OperationCode { get; }
        public Guid CorrelationId { get; }
    }
}