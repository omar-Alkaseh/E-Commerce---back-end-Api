namespace Application.Features.Users.DTOs
{
    public class UserDto
    {
        public int UserId { get; set; }

        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string? PhoneNumber { get; set; }

        public bool IsActive { get; set; }

        public bool IsEmailConfirmed { get; set; }

        public DateTime CreatedAt { get; set; }

        public List<string> Roles { get; set; } = new();
    }
}
