using Application.Features.Auth.DTOs;
using FluentValidation;

namespace Application.Features.Auth.Validators
{
    public class LogoutDtoValidator : AbstractValidator<LogoutDto>
    {
        public LogoutDtoValidator()
        {
            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage("Refresh token is required.");
        }
    }
}
