using Application.Common.Constants;
using Application.Common.Exceptions;
using Application.Common.Models;
using Application.Common.Validation;
using Application.Features.Auth.DTOs;
using Application.Features.Auth.Services.Interfaces;
using Application.Interfaces.Identity;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using FluentValidation;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Application.Features.Auth.Services
{
    public class RegisterService : IRegisterService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IRefreshTokenGenerator _refreshTokenGenerator;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRoleRepository _roleRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IValidator<RegisterDto> _registerValidator;
        private readonly ILogger<RegisterService> _logger;
        private readonly IAuditService _auditService;


        public RegisterService(ICustomerRepository customerRepository,
            IUserRepository userRepository,
            IRefreshTokenRepository refreshTokenRepository,
            IPasswordHasher passwordHasher,
            IJwtTokenGenerator jwtTokenGenerator,
            IRefreshTokenGenerator refreshTokenGenerator,
            IUnitOfWork unitOfWork,
            IRoleRepository roleRepository,
            ICurrentUserService currentUserService,
            IValidator<RegisterDto> registerValidator, ILogger<RegisterService> logger,
            IAuditService auditService)
        {
            _customerRepository = customerRepository;
            _userRepository = userRepository;
            _refreshTokenGenerator = refreshTokenGenerator;
            _refreshTokenRepository = refreshTokenRepository;
            _passwordHasher = passwordHasher;
            _unitOfWork = unitOfWork;
            _roleRepository = roleRepository;
            _currentUserService = currentUserService;
            _jwtTokenGenerator = jwtTokenGenerator;
            _registerValidator = registerValidator;
            _logger = logger;
            _auditService = auditService;
        }

        public async Task<AuthResult> RegisterAsync(RegisterDto request)
        {
            var validationResult = await _registerValidator.ValidateAsync(request);
            validationResult.ThrowIfValidationFails();

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var emailExist = await _userRepository.EmailExistsAsync(request.Email);

                if (emailExist)
                    throw new BadRequestException("Email already exist.");

                var userRole = await _roleRepository.GetByNameAsync(Roles.Customer);

                if (userRole is null)
                    throw new NotFoundException("User Role not found.");

                var passwordHash = _passwordHasher.Hash(request.Password);

                var user = new User
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    PasswordHash = passwordHash,
                    PhoneNumber = request.PhoneNumber,
                    CreatedAt = DateTime.UtcNow,
                };

                var customer = new Customer
                {
                    User = user,
                    CreatedAt = DateTime.UtcNow
                };

                user.Roles.Add(userRole);

                var rawRefreshToken = _refreshTokenGenerator.Generate();
                var refreshTokenHash = _refreshTokenGenerator.HashToken(rawRefreshToken);

                var refreshToken = new RefreshToken
                {
                    User = user,
                    TokenHash = refreshTokenHash,
                    ExpiresAt = DateTime.UtcNow.AddDays(7),
                    CreatedAt = DateTime.UtcNow,
                    CreatedByIp = _currentUserService.IpAddress,
                    IsUsed = false
                };

                await _userRepository.AddAsync(user);
                await _customerRepository.AddAsync(customer);
                await _refreshTokenRepository.AddAsync(refreshToken);

                await _unitOfWork.SaveChangesAsync();

                var roles = user.Roles
                    .Select(r => r.RoleName)
                    .ToList();


                var accessToken = _jwtTokenGenerator.GenerateJwtToken(
                    user.UserId,
                    user.Email,
                    roles
                    );

                await _unitOfWork.CommitTransactionAsync();

                await _auditService.LogAsync(
                    AuditActions.UserCreated,
                    AuditEntities.User,
                    user.UserId,
                    newValues: JsonSerializer.Serialize(new { Role = Roles.Customer }));

                _logger.LogInformation(
                    "User registered successfully. UserId: {UserId}",
                    user.UserId);

                return new AuthResult
                {
                    AccessToken = accessToken.AccessToken,
                    ExpiresAtUtc = accessToken.ExpiresAtUtc,
                    RefreshToken = rawRefreshToken
                };

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Register failed");
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
    }
}
