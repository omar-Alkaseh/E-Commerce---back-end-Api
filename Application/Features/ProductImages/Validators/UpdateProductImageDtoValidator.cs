using Application.Features.ProductImages.DTOs;
using FluentValidation;

namespace Application.Features.ProductImages.Validators
{
    public class UpdateProductImageDtoValidator : AbstractValidator<UpdateProductImageDto>
    {
        private readonly string[] _allowedContentTypes =
        {
            "image/jpeg",
            "image/jpg",
            "image/png",
            "image/webp"
        };

        public UpdateProductImageDtoValidator()
        {
            RuleFor(x => x.ProductId)
                .GreaterThan(0)
                .WithMessage("ProductId must be greater than 0.");

            RuleFor(x => x.ProductImageId)
                .GreaterThan(0)
                .WithMessage("ProductImageId must be greater than 0.");

            RuleFor(x => x.FileName)
                .NotEmpty()
                .WithMessage("File name is required when uploading a new image.")
                .MaximumLength(255)
                .WithMessage("File name must not exceed 255 characters.")
                .Must(HaveValidExtension)
                .WithMessage("Only .jpg, .jpeg, .png, and .webp files are allowed.")
                .When(x => x.FileStream is not null);

            RuleFor(x => x.ContentType)
                .NotEmpty()
                .WithMessage("Content type is required when uploading a new image.")
                .Must(contentType => _allowedContentTypes.Contains(contentType?.ToLower()))
                .WithMessage("Only JPEG, PNG, and WEBP images are allowed.")
                .When(x => x.FileStream is not null);

            RuleFor(x => x.AltText)
                .MaximumLength(250)
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