using Application.Interfaces.Identity;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Identity
{
    public class RefreshTokenGenerator : IRefreshTokenGenerator
    {
        public string Generate()
        {
            var randomBytes = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(randomBytes);
        }

        public string HashToken(string token)
        {
            using var sha256 = SHA256.Create();

            var bytes = Encoding.UTF8.GetBytes(token);
            var hashBytes = sha256.ComputeHash(bytes);

            return Convert.ToBase64String(hashBytes);
        }
    }
}
