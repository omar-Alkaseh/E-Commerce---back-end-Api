using Application.Features.Products.DTOs;
using FluentValidation;

namespace Application.Features.Products.Validators
{
    public class PatchProductDtoValidator : AbstractValidator<PatchProductDto>
    {
        public PatchProductDtoValidator()
        {
            RuleFor(x => x.ProductId)
                .GreaterThan(0).WithMessage("Product id is required.");

            RuleFor(x => x.ProductName)
                .MinimumLength(2).WithMessage("Product name must be at least 2 characters.")
                .MaximumLength(150).WithMessage("Product name must not exceed 150 characters.")
                .When(x => x.ProductName is not null);

            RuleFor(x => x.Sku)
                .MaximumLength(50).WithMessage("SKU must not exceed 50 characters.")
                .When(x => x.Sku is not null);

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.")
                .When(x => x.Description is not null);

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than zero.")
                .When(x => x.Price.HasValue);

            RuleFor(x => x.DiscountPrice)
                .GreaterThan(0).WithMessage("Discount price must be greater than zero.")
                .LessThan(x => x.Price).WithMessage("Discount price must be less than price.")
                .When(x => x.DiscountPrice.HasValue);

            RuleFor(x => x.StockQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("Stock quantity cannot be negative.")
                .When(x => x.StockQuantity.HasValue);

            RuleFor(x => x.CategoryId)
                .GreaterThan(0).WithMessage("Category is required.")
                .When(x => x.CategoryId.HasValue);
        }
    }
}
