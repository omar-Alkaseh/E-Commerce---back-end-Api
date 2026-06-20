using Application.Features.Orders.DTOs;
using FluentValidation;

namespace Application.Features.Orders.Validators
{
    public class OrderStatusValidator : AbstractValidator<OrderStatusDto>
    {
        public OrderStatusValidator()
        {
            RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Invalid order status.");
        }
    }
}
