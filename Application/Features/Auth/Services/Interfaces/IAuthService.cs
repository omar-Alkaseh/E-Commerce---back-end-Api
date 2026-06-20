using Application.Common.Models;
using Application.Features.Auth.DTOs;

namespace Application.Features.Auth.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResult> RegisterAsync(RegisterDto request);
        Task<AuthResult> LoginAsync(LoginDto request);
        Task<AuthResult> RefreshTokenAsync(RefreshTokenRequestDto request);
        Task LogoutAsync(LogoutDto request);
    }
}
