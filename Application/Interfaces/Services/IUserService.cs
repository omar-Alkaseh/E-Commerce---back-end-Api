using Application.Features.Users.DTOs;

namespace Application.Interfaces.Services
{
    public interface IUserService
    {
        Task<IReadOnlyList<UserDto>> GetAllAsync();

        Task<UserDto> GetByIdAsync(int userId);

        Task<UserDto> CreateUserAsync(CreateUserDto request);

        Task<UserDto> CreateAdminAsync(CreateUserDto request);

        Task<UserDto> UpdateAsync(int userId, UpdateUserDto request);

        Task MakeAdminAsync(int userId);

        Task DeleteAsync(int userId);
    }
}
