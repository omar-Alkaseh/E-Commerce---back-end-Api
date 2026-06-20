using Application.Common.Models;
using Application.Common.Validation;
using Application.Common.Constants;
using Application.Features.Auth.DTOs;
using Application.Features.Auth.Services.Interfaces;
using Application.Interfaces.Identity;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using FluentValidation;
using Application.Common.Exceptions;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Application.Features.Auth.Services
{
    public class LoginService : ILoginService
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<LoginDto> _loginValidator;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IRefreshTokenGenerator _refreshTokenGenerator;
        private readonly ILogger<LoginService> _logger;
        private readonly IAuditService _auditService;

        public LoginService(ICurrentUserService currentUserService, IUserRepository userRepository, IRefreshTokenRepository refreshTokenRepository,
            IUnitOfWork unitOfWork, IValidator<LoginDto> loginValidator, IPasswordHasher passwordHasher,
            IJwtTokenGenerator jwtTokenGenerator, IRefreshTokenGenerator refreshTokenGenerator, ILogger<LoginService> logger,
            IAuditService auditService)
        {
            _currentUserService = currentUserService;
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _unitOfWork = unitOfWork;
            _loginValidator = loginValidator;
            _passwordHasher = passwordHasher;
            _jwtTokenGenerator = jwtTokenGenerator;
            _refreshTokenGenerator = refreshTokenGenerator;
            _logger = logger;
            _auditService = auditService;
        }
        public async Task<AuthResult> LoginAsync(LoginDto request)
        {
            var validationResult = await _loginValidator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                await _auditService.LogAsync(
                    AuditActions.LoginFailed,
                    AuditEntities.User,
                    newValues: "{\"Reason\":\"ValidationFailed\"}");
            }

            validationResult.ThrowIfValidationFails();

            var user = await _userRepository.GetByEmailWithRolesAsync(request.Email);

            if (user == null)
            {
                await _auditService.LogAsync(
                    AuditActions.LoginFailed,
                    AuditEntities.User,
                    newValues: "{\"Reason\":\"InvalidCredentials\"}");
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            if (!user.IsActive)
            {
                _logger.LogWarning(
                    "Inactive user attempted login. UserId: {UserId}, IpAddress: {IpAddress}",
                    user.UserId,
                    _currentUserService.IpAddress);

                await _auditService.LogAsync(
                    AuditActions.LoginFailed,
                    AuditEntities.User,
                    user.UserId,
                    newValues: "{\"Reason\":\"InactiveUser\"}");

                throw new ForbiddenException("Invalid email or password.");
            }
               

            if (user.LockoutUntil.HasValue && user.LockoutUntil.Value > DateTime.UtcNow)
            {
                _logger.LogWarning(
                     "Locked user attempted login. UserId: {UserId}, LockoutUntil: {LockoutUntil}, IpAddress: {IpAddress}",
                     user.UserId,
                     user.LockoutUntil,
                     _currentUserService.IpAddress);

                await _auditService.LogAsync(
                    AuditActions.LoginFailed,
                    AuditEntities.User,
                    user.UserId,
                    newValues: "{\"Reason\":\"UserLocked\"}");

                throw new UnauthorizedAccessException("Invalid email or password.");
            }
             

            var isPasswordValid = _passwordHasher.Verify(
                request.Password,
                user.PasswordHash);

            if (!isPasswordValid)
            {
                user.FailedLoginAttempts++;

                if (user.FailedLoginAttempts >= 5)
                {
                    user.LockoutUntil = DateTime.UtcNow.AddMinutes(5);

                    _logger.LogWarning(
                        "User locked after too many failed login attempts. UserId: {UserId}, LockoutUntil: {LockoutUntil}, IpAddress: {IpAddress}",
                        user.UserId,
                        user.LockoutUntil,
                        _currentUserService.IpAddress);
                }

                await _unitOfWork.SaveChangesAsync();

                _logger.LogWarning(
                    "Invalid login attempt. UserId: {UserId}, FailedAttempts: {FailedAttempts}, IpAddress: {IpAddress}",
                    user.UserId,
                    user.FailedLoginAttempts,
                    _currentUserService.IpAddress);

                await _auditService.LogAsync(
                    AuditActions.LoginFailed,
                    AuditEntities.User,
                    user.UserId,
                    newValues: JsonSerializer.Serialize(new
                    {
                        Reason = "InvalidCredentials",
                        FailedAttempts = user.FailedLoginAttempts
                    }));

                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            user.FailedLoginAttempts = 0;
            user.LockoutUntil = null;

            var roles = user.Roles
                .Select(r => r.RoleName)
                .ToList();

            var accessToken = _jwtTokenGenerator.GenerateJwtToken(user.UserId, user.Email, roles);

            var rawRefreshToken = _refreshTokenGenerator.Generate();
            var refreshTokenHash = _refreshTokenGenerator.HashToken(rawRefreshToken);

            var refreshToken = new RefreshToken
            {
                UserId = user.UserId,
                TokenHash = refreshTokenHash,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                CreatedByIp = _currentUserService.IpAddress,
                IsUsed = false
            };

            await _refreshTokenRepository.AddAsync(refreshToken);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "User logged in successfully. UserId: {UserId}, IpAddress: {IpAddress}",
                user.UserId,
                _currentUserService.IpAddress);

            return new AuthResult
            {
                AccessToken = accessToken.AccessToken,
                RefreshToken = rawRefreshToken,
                ExpiresAtUtc = accessToken.ExpiresAtUtc,
            };
        }
    }
}
