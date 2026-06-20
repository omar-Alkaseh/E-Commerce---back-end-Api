using Application.Features.Auth.DTOs;

namespace Application.Features.Auth.Services.Interfaces
{
    public interface ILogoutService
    {
        Task LogoutAsync(LogoutDto request);
    }
}
