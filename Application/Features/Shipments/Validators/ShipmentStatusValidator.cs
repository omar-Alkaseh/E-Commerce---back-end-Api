using Application.Features.Shipments.DTOs;
using FluentValidation;

namespace Application.Features.Shipments.Validators
{
    public class ShipmentStatusValidator : AbstractValidator<ShipmentStatusDto>
    {
        public ShipmentStatusValidator()
        {
            RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Invalid shipment status.");
        }
    }
}
