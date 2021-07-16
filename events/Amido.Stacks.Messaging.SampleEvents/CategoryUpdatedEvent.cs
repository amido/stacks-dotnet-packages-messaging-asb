using System;
using Amido.Stacks.Application.CQRS.ApplicationEvents;
using Amido.Stacks.Core.Operations;

namespace Amido.Stacks.Messaging.SampleEvents
{
	public class CategoryUpdatedEvent : IApplicationEvent
	{
		public CategoryUpdatedEvent(IOperationContext context, Guid menuId, Guid categoryId)
		{
			OperationCode = context.OperationCode;
			CorrelationId = context.CorrelationId;
			MenuId = menuId;
			CategoryId = categoryId;
		}

		public int EventCode => (int)Enums.EventCode.CategoryUpdated;

		public int OperationCode { get; }

		public Guid CorrelationId { get; }

		public Guid MenuId { get; set; }

		public Guid CategoryId { get; set; }
	}
}