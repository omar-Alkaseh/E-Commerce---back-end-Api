using Application.Common.Constants;
using Application.Common.Validation;
using Application.Features.Users.DTOs;
using Application.Interfaces.Identity;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Common.Exceptions;
using FluentValidation;
using Domain.Entities;
using Application.Mappings;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IRoleRepository _roleRepository;
        private readonly IValidator<CreateUserDto> _createUserValidator;
        private readonly IValidator<UpdateUserDto> _updateUserValidator;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<UserService> _logger;
        private readonly IAuditService _auditService;

        public UserService(IUserRepository userRepository, IUnitOfWork unitOfWork, IRoleRepository roleRepository,
            IPasswordHasher passwordHasher, IValidator<CreateUserDto> createUserValidator, IValidator<UpdateUserDto> updateUserValidator,
            ICurrentUserService currentUserService, ILogger<UserService> logger, IAuditService auditService)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _roleRepository = roleRepository;
            _passwordHasher = passwordHasher;
            _createUserValidator = createUserValidator;
            _updateUserValidator = updateUserValidator;
            _currentUserService = currentUserService;
            _logger = logger;
            _auditService = auditService;
        }

        public async Task<UserDto> CreateAdminAsync(CreateUserDto request)
        {
            var validationResult = await _createUserValidator.ValidateAsync(request);
            validationResult.ThrowIfValidationFails();

            return await CreateUserInternalAsync(request, Roles.Admin);
        }

        public async Task<UserDto> CreateUserAsync(CreateUserDto request)
        {
            var validationResult = await _createUserValidator.ValidateAsync(request);
            validationResult.ThrowIfValidationFails();

            return await CreateUserInternalAsync(request, Roles.Customer);
        }

        public async Task DeleteAsync(int userId)
        {
            if (userId <= 0)
            {
                _logger.LogWarning("Cannot deactivate user with invalid id {UserId}", userId);
                throw new BadRequestException("userId must greater than 0.");
            }

            var user = await _userRepository.GetByIdWithRolesAsync(userId);

            if (user is null)
            {
                _logger.LogWarning("Cannot deactivate user {UserId} because the user was not found", userId);
                throw new NotFoundException("User not found.");
            }

            var targetIsAdmin = user.Roles.Any(r => r.RoleName == Roles.Admin);
            var targetIsSuperAdmin = user.Roles.Any(r => r.RoleName == Roles.SuperAdmin);

            var currentUserIsAdmin = _currentUserService.Roles.Contains(Roles.Admin);
            var currentUserIsSuperAdmin = _currentUserService.Roles.Contains(Roles.SuperAdmin);

            if (targetIsSuperAdmin)
            {
                _logger.LogWarning("Attempted to deactivate SuperAdmin user {UserId}", userId);
                throw new BadRequestException("SuperAdmin cannot be deactivated.");
            }

            if (targetIsAdmin && currentUserIsAdmin && !currentUserIsSuperAdmin)
            {
                _logger.LogWarning("Admin attempted to deactivate another admin user {UserId}", userId);
                throw new ForbiddenException("Admin cannot delete another admin.");
            }


            var wasActive = user.IsActive;

            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;

            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync();

            await _auditService.LogAsync(
                AuditActions.UserDeleted,
                AuditEntities.User,
                userId,
                oldValues: JsonSerializer.Serialize(new { IsActive = wasActive }),
                newValues: JsonSerializer.Serialize(new { IsActive = false }));

            _logger.LogInformation("User {UserId} deactivated", userId);
        }

        public async Task<IReadOnlyList<UserDto>> GetAllAsync()
        {
            var users = await _userRepository.GetAllWithRolesAsync();

            var currentUserIsSuperAdmin = _currentUserService.Roles.Contains(Roles.SuperAdmin);

            if (!currentUserIsSuperAdmin)
            {
                users = users.Where(u => !u.Roles.Any(r => r.RoleName == Roles.SuperAdmin)).ToList();
            }

            return users.ToDtoList();
        }

        public async Task<UserDto> GetByIdAsync(int userId)
        {
            if (userId <= 0)
            {
                _logger.LogWarning("Cannot get user with invalid id {UserId}", userId);
                throw new BadRequestException("userId must greater than 0.");
            }

            var user = await _userRepository.GetByIdWithRolesAsync(userId);

            if (user is null)
            {
                _logger.LogWarning("User {UserId} was not found", userId);
                throw new NotFoundException("User not found.");
            }

            var targetIsSuperAdmin = user.Roles.Any(r => r.RoleName == Roles.SuperAdmin);
            var currentUserIsSuperAdmin = _currentUserService.Roles.Contains(Roles.SuperAdmin);

            if (targetIsSuperAdmin && !currentUserIsSuperAdmin)
            {
                _logger.LogWarning("Admin attempted to access SuperAdmin user {UserId}", userId);
                throw new ForbiddenException("Admin cannot access SuperAdmin user.");
            }

            return user.ToDto();
        }

        public async Task MakeAdminAsync(int userId)
        {
            if (userId <= 0)
            {
                _logger.LogWarning("Cannot promote user with invalid id {UserId}", userId);
                throw new BadRequestException("userId must greater than 0.");
            }

            var user = await _userRepository.GetByIdWithRolesAsync(userId);

            if (user is null)
            {
                _logger.LogWarning("Cannot promote user {UserId} because the user was not found", userId);
                throw new NotFoundException("User not found.");
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("Cannot promote inactive user {UserId} to admin", userId);
                throw new BadRequestException("Inactive user cannot be promoted to Admin.");
            }

            var targetIsSuperAdmin = user.Roles.Any(r => r.RoleName == Roles.SuperAdmin);

            if (targetIsSuperAdmin)
            {
                _logger.LogWarning("Attempted to modify SuperAdmin user {UserId} to admin", userId);
                throw new BadRequestException("SuperAdmin cannot be modified to Admin.");
            }


            var adminRole = await _roleRepository.GetByNameAsync(Roles.Admin);

            if (adminRole is null)
            {
                _logger.LogWarning("Cannot promote user {UserId} because the admin role was not found", userId);
                throw new NotFoundException("Admin role not found.");
            }


            var alreadyAdmin = user.Roles.Any(c => c.RoleName == Roles.Admin);

            if (alreadyAdmin)
                return;

            user.Roles.Add(adminRole);
            user.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("User {UserId} promoted to admin", userId);

        }

        public async Task<UserDto> UpdateAsync(int userId, UpdateUserDto request)
        {
            var validationResult = await _updateUserValidator.ValidateAsync(request);
            validationResult.ThrowIfValidationFails();

            if (userId <= 0)
            {
                _logger.LogWarning("Cannot update user with invalid id {UserId}", userId);
                throw new BadRequestException("userId must greater than 0.");
            }

            var user = await _userRepository.GetByIdWithRolesAsync(userId);

            if (user is null)
            {
                _logger.LogWarning("Cannot update user {UserId} because the user was not found", userId);
                throw new NotFoundException("User not found.");
            }

            var targetIsAdmin = user.Roles.Any(r => r.RoleName == Roles.Admin);
            var targetIsSuperAdmin = user.Roles.Any(r => r.RoleName == Roles.SuperAdmin);

            var currentUserIsAdmin = _currentUserService.Roles.Contains(Roles.Admin);
            var currentUserIsSuperAdmin = _currentUserService.Roles.Contains(Roles.SuperAdmin);

            if (targetIsAdmin && currentUserIsAdmin && !currentUserIsSuperAdmin)
            {
                _logger.LogWarning("Admin attempted to update another admin user {UserId}", userId);
                throw new ForbiddenException("Admin cannot update another admin.");
            }


            if (targetIsSuperAdmin && !currentUserIsSuperAdmin)
            {
                _logger.LogWarning("Admin attempted to update SuperAdmin user {UserId}", userId);
                throw new ForbiddenException("Admin cannot update SuperAdmin user.");
            }


            if (targetIsSuperAdmin && request.IsActive == false)
            {
                _logger.LogWarning("Attempted to deactivate SuperAdmin user {UserId}", userId);
                throw new BadRequestException("SuperAdmin cannot be deactivated.");
            }


            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.PhoneNumber = request.PhoneNumber;
            user.IsActive = request.IsActive;
            user.UpdatedAt = DateTime.UtcNow;

            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("User {UserId} updated", userId);

            return user.ToDto();
        }


        private async Task<UserDto> CreateUserInternalAsync(CreateUserDto request, string roleName)
        {
            var emailExists = await _userRepository.EmailExistsAsync(request.Email);

            if (emailExists)
            {
                _logger.LogWarning("Cannot create user because the email already exists");
                throw new BadRequestException("Email already exists.");
            }

            var role = await _roleRepository.GetByNameAsync(roleName);

            if (role is null)
            {
                _logger.LogWarning("Cannot create user because role {RoleName} was not found", roleName);
                throw new NotFoundException($"{roleName} role not found.");
            }

            var user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                PasswordHash = _passwordHasher.Hash(request.Password),
                IsActive = true,
                IsEmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            user.Customer = new Customer
            {
                CreatedAt = DateTime.UtcNow
            };

            var customerRole = await _roleRepository.GetByNameAsync(Roles.Customer);

            if (customerRole is null)
            {
                _logger.LogWarning("Cannot create user because the customer role was not found");
                throw new NotFoundException($"{customerRole?.RoleName} role not found.");
            }

            user.Roles.Add(customerRole);

            if (roleName != Roles.Customer)
            {
                user.Roles.Add(role);
            }

            await _userRepository.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            var auditAction = roleName == Roles.Admin
                ? AuditActions.AdminCreated
                : AuditActions.UserCreated;

            await _auditService.LogAsync(
                auditAction,
                AuditEntities.User,
                user.UserId,
                newValues: JsonSerializer.Serialize(new { Role = roleName }));

            _logger.LogInformation("User {UserId} created with role {RoleName}", user.UserId, roleName);

            return user.ToDto();
        }
    }
}
