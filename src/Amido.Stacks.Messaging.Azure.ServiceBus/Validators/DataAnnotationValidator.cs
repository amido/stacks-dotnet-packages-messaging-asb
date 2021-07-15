using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Amido.Stacks.Core.Operations;
using Amido.Stacks.Messaging.Azure.ServiceBus.Exceptions;

namespace Amido.Stacks.Messaging.Azure.ServiceBus.Validators
{
    public class DataAnnotationValidator : IValidator<IOperationContext>
    {
        public void Validate(IOperationContext message)
        {
            var context = new ValidationContext(message);
            var results = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(message, context, results, true);

            if (!isValid)
                throw new InvalidMessageBodyException(message.CorrelationId,
                    $"Invalid message: {string.Join(";", results?.Select(r => r.ErrorMessage))}");
        }
    }
}