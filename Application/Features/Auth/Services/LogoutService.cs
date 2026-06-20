using Application.Common.Exceptions;
using Application.Common.Validation;
using Application.Features.Auth.DTOs;
using Application.Features.Auth.Services.Interfaces;
using Application.Interfaces.Identity;
using Application.Interfaces.Repositories;
using FluentValidation;

namespace Application.Features.Auth.Services
{
    public class LogoutService : ILogoutService
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<LogoutDto> _logoutValidator;
        private readonly IRefreshTokenGenerator _refreshTokenGenerator;

        public LogoutService(ICurrentUserService currentUserService, IRefreshTokenRepository refreshTokenRepository,
            IUnitOfWork unitOfWork, IValidator<LogoutDto> logoutValidator, IRefreshTokenGenerator refreshTokenGenerator)
        {
            _currentUserService = currentUserService;
            _refreshTokenRepository = refreshTokenRepository;
            _unitOfWork = unitOfWork;
            _logoutValidator = logoutValidator;
            _refreshTokenGenerator = refreshTokenGenerator;
        }


        public async Task LogoutAsync(LogoutDto request)
        {
            var validationResult = await _logoutValidator.ValidateAsync(request);
            validationResult.ThrowIfValidationFails();

            if (string.IsNullOrWhiteSpace(request.RefreshToken))
                throw new BadRequestException("Refresh token is required.");

            var tokenHash = _refreshTokenGenerator.HashToken(request.RefreshToken);

            var storedToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash);

            if (storedToken is null)
                return;

            if (storedToken.RevokedAt is not null)
                return;

            storedToken.RevokedAt = DateTime.UtcNow;
            storedToken.RevokedByIp = _currentUserService.IpAddress;

            _refreshTokenRepository.Update(storedToken);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
