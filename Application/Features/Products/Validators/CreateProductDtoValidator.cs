using Application.Features.Products.DTOs;
using FluentValidation;

namespace Application.Features.Products.Validators
{
    public class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
    {
        public CreateProductDtoValidator()
        {
            RuleFor(x => x.ProductName)
                .NotEmpty().WithMessage("Product name is required.")
                .MaximumLength(150).WithMessage("Product name must not exceed 150 characters.");

            RuleFor(x => x.Sku)
                .MaximumLength(50).WithMessage("SKU must not exceed 50 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.Sku));

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.Description));

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than zero.");

            RuleFor(x => x.DiscountPrice)
                .GreaterThan(0).WithMessage("Discount price must be greater than zero.")
                .LessThan(x => x.Price).WithMessage("Discount price must be less than price.")
                .When(x => x.DiscountPrice.HasValue);

            RuleFor(x => x.StockQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("Stock quantity cannot be negative.");

            RuleFor(x => x.CategoryId)
                .GreaterThan(0).WithMessage("Category is required.");
        }
    }
}
