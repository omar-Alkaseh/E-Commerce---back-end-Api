using Application.Features.Carts.DTOs;
using FluentValidation;

namespace Application.Features.Carts.Validators
{
    public class UpdateCartItemQuantityDtoValidator : AbstractValidator<UpdateCartItemQuantityDto>
    {
        public UpdateCartItemQuantityDtoValidator()
        {
            RuleFor(x => x.Quantity)
                .GreaterThan(0)
                .WithMessage("Quantity must be greater than zero.");
        }
    }
}
