using Application.Common.Models;
using Application.Common.Validation;
using Application.Features.Auth.DTOs;
using Application.Features.Auth.Services.Interfaces;
using Application.Interfaces.Identity;
using Application.Interfaces.Repositories;
using Domain.Entities;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace Application.Features.Auth.Services
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<RefreshTokenRequestDto> _refreshTokenValidator;
        private readonly IRefreshTokenGenerator _refreshTokenGenerator;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly ILogger<RefreshTokenService> _logger;

        public RefreshTokenService(ICurrentUserService currentUserService, IRefreshTokenRepository refreshTokenRepository,
            IUnitOfWork unitOfWork, IValidator<RefreshTokenRequestDto> refreshTokenValidator, IRefreshTokenGenerator refreshTokenGenerator,
            IJwtTokenGenerator jwtTokenGenerator, ILogger<RefreshTokenService> logger)
        {
            _currentUserService = currentUserService;
            _refreshTokenRepository = refreshTokenRepository;
            _unitOfWork = unitOfWork;
            _refreshTokenValidator = refreshTokenValidator;
            _refreshTokenGenerator = refreshTokenGenerator;
            _jwtTokenGenerator = jwtTokenGenerator;
            _logger = logger;
        }
        public async Task<AuthResult> RefreshTokenAsync(RefreshTokenRequestDto request)
        {
            var validationResult = await _refreshTokenValidator.ValidateAsync(request);
            validationResult.ThrowIfValidationFails();

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                if (string.IsNullOrWhiteSpace(request.RefreshToken))
                    throw new UnauthorizedAccessException("Refresh token is required.");

                var tokenHash = _refreshTokenGenerator.HashToken(request.RefreshToken);

                var storedToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash);

                if (storedToken is null)
                    throw new UnauthorizedAccessException("Invalid refresh token.");

                if (storedToken.RevokedAt is not null)
                {
                    _logger.LogWarning(
                    "Refresh token has been revoked. IpAddress: {IpAddress}",
                    _currentUserService.IpAddress);
                    throw new UnauthorizedAccessException("Refresh token has been revoked.");
                }
                    

                if (storedToken.ExpiresAt <= DateTime.UtcNow)
                {
                    _logger.LogWarning(
                    "Refresh token rejected because it expired. UserId: {UserId}, ExpiredAt: {ExpiredAt}, IpAddress: {IpAddress}",
                    storedToken.UserId,
                    storedToken.ExpiresAt,
                    _currentUserService.IpAddress);
                    throw new UnauthorizedAccessException("Refresh token has expired.");
                }

                var user = storedToken.User;

                var roles = user.Roles
                     .Select(r => r.RoleName)
                     .ToList();

                var accessToken = _jwtTokenGenerator.GenerateJwtToken(user.UserId, user.Email, roles);

                var rawRefreshToken = _refreshTokenGenerator.Generate();
                var refreshTokenHash = _refreshTokenGenerator.HashToken(rawRefreshToken);

                storedToken.IsUsed = true;
                storedToken.RevokedAt = DateTime.UtcNow;
                storedToken.RevokedByIp = _currentUserService.IpAddress;
                storedToken.ReplacedByTokenHash = refreshTokenHash;

                var refreshToken = new RefreshToken
                {
                    User = user,
                    TokenHash = refreshTokenHash,
                    ExpiresAt = DateTime.UtcNow.AddDays(7),
                    CreatedAt = DateTime.UtcNow,
                    CreatedByIp = _currentUserService.IpAddress,
                    IsUsed = false
                };

                _refreshTokenRepository.Update(storedToken);
                await _refreshTokenRepository.AddAsync(refreshToken);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation(
                "Refresh token rotated successfully. UserId: {UserId}, IpAddress: {IpAddress}",
                user.UserId,
                _currentUserService.IpAddress);

                return new AuthResult
                {
                    AccessToken = accessToken.AccessToken,
                    ExpiresAtUtc = accessToken.ExpiresAtUtc,
                    RefreshToken = rawRefreshToken
                };
            }
            catch(Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Refresh token process failed. IpAddress: {IpAddress}",
                    _currentUserService.IpAddress);
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
    }
}
