namespace Application.Common.Models
{
    public class AuthResult
    {
        public string AccessToken { get; set; } = null!;
        public DateTime ExpiresAtUtc { get; set; }
        public string RefreshToken { get; set; } = null!;
    }
}
