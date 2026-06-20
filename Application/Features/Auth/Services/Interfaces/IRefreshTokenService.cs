using Application.Common.Models;
using Application.Features.Auth.DTOs;

namespace Application.Features.Auth.Services.Interfaces
{
    public interface IRefreshTokenService
    {
        Task<AuthResult> RefreshTokenAsync(RefreshTokenRequestDto request);
    }
}
