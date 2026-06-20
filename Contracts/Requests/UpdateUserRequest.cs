namespace Contracts.Requests
{
    public class UpdateUserRequest
    {
        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public string? PhoneNumber { get; set; }

        public bool IsActive { get; set; }
    }
}
