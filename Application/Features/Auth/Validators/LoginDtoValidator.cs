using Application.Features.Auth.DTOs;
using FluentValidation;

namespace Application.Features.Auth.Validators
{
    public class LoginDtoValidator : AbstractValidator<LoginDto>
    {
        public LoginDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Email format is invalid.")
                .MaximumLength(150).WithMessage("Email must not exceed 150 characters.")
                .MinimumLength(13).WithMessage("Email must be at least 13 characters.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.");
        }
    }
}
