namespace Contracts.Responses
{
    public class AuthResponse
    {
        public string AccessToken { get; set; } = null!;
        public DateTime ExpiresAtUtc { get; set; }
        public string RefreshToken { get; set; } = null!;
    }
}
