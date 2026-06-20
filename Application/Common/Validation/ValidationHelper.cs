using Application.Common.Exceptions;
using FluentValidation.Results;

namespace Application.Common.Validation
{
    public static class ValidationHelper
    {
        public static void ThrowIfValidationFails(this ValidationResult validationResult)
        {
            if (validationResult.IsValid)
                return;

            var errors = validationResult.Errors
                .GroupBy(p => p.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );

            throw new AppValidationException(errors);
        }
    }
}
