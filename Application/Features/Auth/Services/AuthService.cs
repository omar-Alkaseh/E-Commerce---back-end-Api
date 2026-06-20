using Application.Common.Models;
using Application.Features.Auth.DTOs;
using Application.Features.Auth.Services.Interfaces;

namespace Application.Features.Auth.Services
{
    public class AuthService : IAuthService
    {
        private readonly ILoginService _loginService;
        private readonly ILogoutService _logoutService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IRegisterService _registerService;

        public AuthService(ILoginService loginService, ILogoutService logoutService, IRefreshTokenService refreshTokenService,
            IRegisterService registerService)
        {
            _loginService = loginService;
            _logoutService = logoutService;
            _refreshTokenService = refreshTokenService;
            _registerService = registerService;
        }

        public Task LogoutAsync(LogoutDto request) =>
            _logoutService.LogoutAsync(request);

        public Task<AuthResult> LoginAsync(LoginDto request) =>
            _loginService.LoginAsync(request);

        public Task<AuthResult> RefreshTokenAsync(RefreshTokenRequestDto request) =>
            _refreshTokenService.RefreshTokenAsync(request);
        
        public Task<AuthResult> RegisterAsync(RegisterDto request) =>
            _registerService.RegisterAsync(request);
        
    }
}