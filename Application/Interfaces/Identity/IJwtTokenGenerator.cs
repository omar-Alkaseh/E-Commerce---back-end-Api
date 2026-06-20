using Application.Common.Models;
namespace Application.Interfaces.Identity
{
    public interface IJwtTokenGenerator
    {
        JwtTokenResult GenerateJwtToken(int userId, string email, IEnumerable<string> roles);
    }
}
