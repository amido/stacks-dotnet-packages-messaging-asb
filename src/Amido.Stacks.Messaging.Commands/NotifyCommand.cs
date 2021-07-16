using System;
using Amido.Stacks.Application.CQRS.Commands;

namespace Amido.Stacks.Messaging.Commands
{
    public class NotifyCommand : ICommand
    {
        public NotifyCommand(Guid correlationId, string testMember)
        {
            OperationCode = 666;
            CorrelationId = correlationId;
            TestMember = testMember;
        }

        public string TestMember { get; }
        public int OperationCode { get; }
        public Guid CorrelationId { get; }
    }
}
