using Application.Features.Reviews.DTOs;
using FluentValidation;

namespace Application.Features.Reviews.Validators
{
    public class CreateReviewDtoValidator : AbstractValidator<CreateReviewDto>
    {
        public CreateReviewDtoValidator()
        {
            RuleFor(x => x.Comment)
                  .MaximumLength(500).WithMessage("Comment must not exceed 500 characters.")
                  .When(x => !string.IsNullOrWhiteSpace(x.Comment));

            RuleFor(x => x.Rating)
                .InclusiveBetween(1, 5).WithMessage("Rating must between 1 and 5.");
        }
    }
}
