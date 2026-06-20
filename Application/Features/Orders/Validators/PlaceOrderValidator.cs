using Application.Features.Orders.DTOs;
using FluentValidation;

namespace Application.Features.Orders.Validators
{
    public class PlaceOrderValidator : AbstractValidator<PlaceOrderDto>
    {
        public PlaceOrderValidator()
        {
            RuleFor(x => x.ShippingAddressLine)
                .NotEmpty().WithMessage("Shipping address line is required.")
                .MaximumLength(250).WithMessage("Shipping address line must not exceed 250 characters.");

            RuleFor(x => x.ShippingCity)
                .NotEmpty().WithMessage("Shipping city is required.")
                .MaximumLength(100).WithMessage("Shipping city must not exceed 100 characters.");

            RuleFor(x => x.ShippingCountry)
                .NotEmpty().WithMessage("Shipping country is required.")
                .MaximumLength(100).WithMessage("Shipping country must not exceed 100 characters.");

            RuleFor(x => x.ShippingPostalCode)
                .MaximumLength(20).WithMessage("Shipping postal code must not exceed 20 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.ShippingPostalCode));
        }
    }
}
