using Application.Interfaces.Services;
using Microsoft.Extensions.Hosting;

namespace Infrastructure.Services
{
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly IHostEnvironment _environment;

        private static readonly string[] AllowedContentTypes =
        {
            "image/jpeg",
            "image/png",
            "image/webp"
        };

        private static readonly string[] AllowedExtensions =
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp"
        };

        public LocalFileStorageService(IHostEnvironment environment)
        {
            _environment = environment;
        }

        public Task DeleteFileAsync(string fileUrl)
        {
            if (string.IsNullOrWhiteSpace(fileUrl))
                return Task.CompletedTask;

            var relativePath = fileUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);

            var rootPath = _environment.ContentRootPath;
            var webRootPath = Path.Combine(rootPath, "wwwroot");

            var fullPath = Path.Combine(webRootPath, relativePath);

            if (File.Exists(fullPath))
                File.Delete(fullPath);

            return Task.CompletedTask;
        }

        public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string contentType, string folderName)
        {
            if (!AllowedContentTypes.Contains(contentType))
                throw new InvalidOperationException("Only JPG, PNG, and WEBP images are allowed.");

            var extension = Path.GetExtension(fileName).ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
                throw new InvalidOperationException("Invalid image extension.");

            var rootPath = _environment.ContentRootPath;
            var webRootPath = Path.Combine(rootPath, "wwwroot");

            var uploadsFolder = Path.Combine(webRootPath, "uploads", folderName);

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            await using var outputStream = new FileStream(filePath, FileMode.Create);
            await fileStream.CopyToAsync(outputStream);

            return $"/uploads/{folderName}/{uniqueFileName}";
        }
    }
}
