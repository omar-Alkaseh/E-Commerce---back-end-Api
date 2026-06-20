using Application.Features.Users.DTOs;
using FluentValidation;

namespace Application.Features.Users.Validator
{
    public class UpdateUserValidator : AbstractValidator<UpdateUserDto>
    {
        public UpdateUserValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required.")
                .MaximumLength(50).WithMessage("First name must not exceed 50 characters.")
                .MinimumLength(3).WithMessage("First name should be at least exceed 3 characters.")
                .Matches(@"^[A-Za-z\u0600-\u06FF\s]+$")
                .WithMessage("First name must contain letters only.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .MaximumLength(50).WithMessage("Last name must not exceed 50 characters.")
                .MinimumLength(3).WithMessage("Last name must be at least 3 characters.")
                .Matches(@"^[A-Za-z\u0600-\u06FF\s]+$")
                .WithMessage("Last name must contain letters only.");

            RuleFor(x => x.PhoneNumber)
                .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters.")
                .Matches(@"^\+?[0-9\s\-]+$")
                .WithMessage("Phone number must contain numbers only.")
                .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));
        }
    }
}
