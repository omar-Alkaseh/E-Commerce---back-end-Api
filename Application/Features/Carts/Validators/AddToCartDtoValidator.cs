using Application.Features.Carts.DTOs;
using FluentValidation;

namespace Application.Features.Carts.Validators
{
    public class AddToCartDtoValidator : AbstractValidator<AddToCartDto>
    {
        public AddToCartDtoValidator() 
        {
            RuleFor(x => x.ProductId)
                .GreaterThan(0)
                .WithMessage("ProductId is required.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0)
                .WithMessage("Quantity must be greater than zero.");
        }
    }
}
