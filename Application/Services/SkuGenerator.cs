using Application.Interfaces.Services;

using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class SkuGenerator : ISkuGenerator
    {
        private readonly ILogger<SkuGenerator> _logger;

        public SkuGenerator(ILogger<SkuGenerator> logger)
        {
            _logger = logger;
        }

        public string GenerateSku(string productName, int categoryId)
        {
            var prefix = GeneratePrefix(productName);

            var datePart = DateTime.UtcNow.ToString("yyyyMMdd");

            var randomPart = Random.Shared.Next(1000, 9999);

            return $"{prefix}-{categoryId}-{datePart}-{randomPart}";
        }

        private static string GeneratePrefix(string productName)
        {
            if (string.IsNullOrWhiteSpace(productName))
                return "PRD";

            var cleanName = productName.Trim();

            var letter = cleanName
                .Where(char.IsLetterOrDigit)
                .Take(3)
                .ToArray();

            if (letter.Length == 0)
                return "PRD";

            return new string(letter).ToLowerInvariant();
        }
    }
}
