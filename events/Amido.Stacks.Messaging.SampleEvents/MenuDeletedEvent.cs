using System;
using Amido.Stacks.Application.CQRS.ApplicationEvents;
using Amido.Stacks.Core.Operations;

namespace Amido.Stacks.Messaging.SampleEvents
{
	public class MenuDeletedEvent : IApplicationEvent
	{
		public MenuDeletedEvent(IOperationContext context, Guid menuId)
		{
			OperationCode = context.OperationCode;
			CorrelationId = context.CorrelationId;
			MenuId = menuId;
		}

		public int EventCode => (int)Enums.EventCode.MenuDeleted;

		public int OperationCode { get; }

		public Guid CorrelationId { get; }

		public Guid MenuId { get; set; }
	}
}