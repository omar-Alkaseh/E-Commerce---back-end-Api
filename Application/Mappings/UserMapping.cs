using Application.Features.Users.DTOs;
using Domain.Entities;

namespace Application.Mappings
{
    public static class UserMapping
    {
        public static UserDto ToDto(this User user)
        {
            return new UserDto
            {
                UserId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                IsActive = user.IsActive,
                IsEmailConfirmed = user.IsEmailConfirmed,
                CreatedAt = user.CreatedAt,
                Roles = user.Roles.Select(r => r.RoleName).ToList()
            };
        }

        public static List<UserDto> ToDtoList(this IEnumerable<User> users)
        {
            return users.Select(u => u.ToDto()).ToList();
        }
    }
}
