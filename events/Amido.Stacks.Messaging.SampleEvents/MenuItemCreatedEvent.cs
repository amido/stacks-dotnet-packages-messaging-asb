using System;
using Amido.Stacks.Application.CQRS.ApplicationEvents;
using Amido.Stacks.Core.Operations;

namespace Amido.Stacks.Messaging.SampleEvents
{
	public class MenuItemCreatedEvent : IApplicationEvent
	{
		public MenuItemCreatedEvent(IOperationContext context, Guid menuId, Guid categoryId, Guid menuItemId)
		{
			OperationCode = context.OperationCode;
			CorrelationId = context.CorrelationId;
			MenuId = menuId;
			CategoryId = categoryId;
			MenuItemId = menuItemId;
		}

		public int EventCode => (int)Enums.EventCode.MenuItemCreated;

		public int OperationCode { get; }

		public Guid CorrelationId { get; }

		public Guid MenuId { get; set; }

		public Guid CategoryId { get; set; }

		public Guid MenuItemId { get; set; }
	}
}
