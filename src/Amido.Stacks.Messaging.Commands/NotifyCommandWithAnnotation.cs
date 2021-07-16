using System;
using System.ComponentModel.DataAnnotations;
using Amido.Stacks.Application.CQRS.Commands;

namespace Amido.Stacks.Messaging.Commands
{
    public class NotifyCommandWithAnnotation : ICommand
    {
        public NotifyCommandWithAnnotation(Guid correlationId, string testProperty)
        {
            CorrelationId = correlationId;
            TestProperty = testProperty;
        }

        public int OperationCode => 536;

        [Required]
        public Guid CorrelationId { get; }

        [Required]
        public string TestProperty { get; }
    }
}