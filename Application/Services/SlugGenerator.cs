using Application.Interfaces.Services;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class SlugGenerator : ISlugGenerator
    {
        private readonly ILogger<SlugGenerator> _logger;

        public SlugGenerator(ILogger<SlugGenerator> logger)
        {
            _logger = logger;
        }

        public string GenerateSlug(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            value = value.Trim().ToLowerInvariant();

            value = Regex.Replace(value, @"\s+", "-");

            value = Regex.Replace(value, @"[^\p{L}\p{N}-]", "");

            value = Regex.Replace(value, @"-+", "-");

            value = value.Trim('-');

            return value;
        }
    }
}
