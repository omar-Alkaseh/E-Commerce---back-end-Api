using Application.Features.Categories.DTOs;
using FluentValidation;

namespace Application.Features.Categories.Validators
{
    public class UpdateCategoryRequestValidator : AbstractValidator<UpdateCategoryDto>
    {
        public UpdateCategoryRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Category name is required.")
                .MaximumLength(100)
                .WithMessage("Category name must not exceed 100 characters.");

            RuleFor(x => x.Slug)
                .NotEmpty()
                .WithMessage("Slug is required.")
                .MaximumLength(120)
                .WithMessage("Slug must not exceed 120 characters.")
                .Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$")
                .WithMessage("Slug must contain only lowercase letters, numbers, and hyphens. Example: gaming-laptops");

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .WithMessage("Description must not exceed 500 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.Description));

            RuleFor(x => x.ParentCategoryId)
                .GreaterThan(0)
                .WithMessage("ParentCategoryId must be greater than 0.")
                .When(x => x.ParentCategoryId.HasValue);
        } 
    }
}
