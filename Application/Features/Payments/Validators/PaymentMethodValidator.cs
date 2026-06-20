using Application.Features.Payments.DTOs;
using Domain.Enums;
using FluentValidation;

namespace Application.Features.Payments.Validators
{
    public class PaymentMethodValidator : AbstractValidator<CreatePaymentDto>
    {
        public PaymentMethodValidator()
        {
            RuleFor(x => x.PaymentMethod)
            .IsInEnum()
            .WithMessage("Invalid payment method.");
        }
    }
}
