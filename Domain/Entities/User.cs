namespace Domain.Entities
{
    public class User
    {
        public int UserId { get; set; }

        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string PasswordHash { get; set; } = null!;

        public bool IsActive { get; set; } = true;
        public bool IsEmailConfirmed { get; set; } = false;
        public int FailedLoginAttempts { get; set; } = 0;
        public DateTime? LockoutUntil { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public Customer? Customer { get; set; }

        public ICollection<Role> Roles { get; set; } = new List<Role>();
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}
