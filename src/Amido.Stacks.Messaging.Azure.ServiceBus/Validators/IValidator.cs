using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Amido.Stacks.Core.Operations;

namespace Amido.Stacks.Messaging.Azure.ServiceBus.Validators
{
    public interface IValidator<in T> where T: IOperationContext
    {
        void Validate(T message);
    }
}