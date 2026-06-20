using Application.Features.Shipments.DTOs;
using FluentValidation;

namespace Application.Features.Shipments.Validators
{
    public class CreateShipmentDtoValidator : AbstractValidator<CreateShipmentDto>
    {
        public CreateShipmentDtoValidator()
        {
            RuleFor(x => x.CarrierName)
                .MaximumLength(50).WithMessage("Carrier name number must not exceed 50 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.CarrierName));
        }
    }
}
