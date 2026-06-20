using Application.Features.Users.DTOs;
using FluentValidation;

namespace Application.Features.Users.Validator
{
    public class CreateUserValidator : AbstractValidator<CreateUserDto>
    {
        public CreateUserValidator()
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

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Email format is invalid.")
                .MaximumLength(150).WithMessage("Email must not exceed 150 characters.")
                .MinimumLength(13).WithMessage("Email must be at least 13 characters.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is rquired.")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters.")
                .MaximumLength(100).WithMessage("Password must not exceed 100 characters.");

            RuleFor(x => x.PhoneNumber)
                .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters.")
                .Matches(@"^\+?[0-9\s\-]+$")
                .WithMessage("Phone number must contain numbers only.")
                .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));
        }
    }
}
