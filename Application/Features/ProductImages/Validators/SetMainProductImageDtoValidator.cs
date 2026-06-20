using Application.Features.ProductImages.DTOs;
using FluentValidation;

namespace Application.Features.ProductImages.Validators
{
    public class SetMainProductImageDtoValidator : AbstractValidator<SetMainProductImageDto>
    {
        public SetMainProductImageDtoValidator()
        {
            RuleFor(x => x.ProductId)
                .GreaterThan(0)
                .WithMessage("ProductId must be greater than 0.");

            RuleFor(x => x.ProductImageId)
                .GreaterThan(0)
                .WithMessage("ProductImageId must be greater than 0.");
        }
    }
}