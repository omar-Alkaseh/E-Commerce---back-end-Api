using Application.Features.Shipments.DTOs;
using FluentValidation;

namespace Application.Features.Shipments.Validators
{
    public class UpdateShipmentDtoValidator : AbstractValidator<UpdateShipmentDto>
    {
        public UpdateShipmentDtoValidator()
        {
            RuleFor(x => x.TrackingNumber)
                .MaximumLength(100).WithMessage("Traking number must not exceed 100 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.TrackingNumber));

            RuleFor(x => x.CarrierName)
                .MaximumLength(50).WithMessage("Carrier name number must not exceed 50 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.CarrierName));
        }
    }
}
