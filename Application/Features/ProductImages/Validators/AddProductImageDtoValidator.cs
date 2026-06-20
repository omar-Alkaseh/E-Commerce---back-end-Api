using Application.Features.ProductImages.DTOs;
using FluentValidation;

namespace Application.Features.ProductImages.Validators
{
    public class AddProductImageDtoValidator : AbstractValidator<AddProductImageDto>
    {
        private readonly string[] _allowedContentTypes =
        {
            "image/jpeg",
            "image/jpg",
            "image/png",
            "image/webp"
        };

        public AddProductImageDtoValidator()
        {
            RuleFor(x => x.ProductId)
                .GreaterThan(0)
                .WithMessage("ProductId must be greater than 0.");

            RuleFor(x => x.FileStream)
                .NotNull()
                .WithMessage("Image file is required.");

            RuleFor(x => x.FileName)
                .NotEmpty()
                .WithMessage("File name is required.")
                .MaximumLength(255)
                .WithMessage("File name must not exceed 255 characters.")
                .Must(HaveValidExtension)
                .WithMessage("Only .jpg, .jpeg, .png, and .webp files are allowed.");

            RuleFor(x => x.ContentType)
                .NotEmpty()
                .WithMessage("Content type is required.")
                .Must(contentType => _allowedContentTypes.Contains(contentType.ToLower()))
                .WithMessage("Only JPEG, PNG, and WEBP images are allowed.");

            RuleFor(x => x.AltText)
                .MaximumLength(255)
                .WithMessage("Alt text must not exceed 250 characters.");

        }

        private static bool HaveValidExtension(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return false;

            var extension = Path.GetExtension(fileName).ToLower();

            return extension is ".jpg" or ".jpeg" or ".png" or ".webp";
        }
    }
}