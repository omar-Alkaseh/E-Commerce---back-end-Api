using Application.Common.Models;
using Application.Features.Auth.DTOs;

namespace Application.Features.Auth.Services.Interfaces
{
    public interface IRegisterService
    {
        Task<AuthResult> RegisterAsync(RegisterDto request);
    }
}
