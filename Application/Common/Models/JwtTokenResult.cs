namespace Application.Common.Models
{
    public class JwtTokenResult
    {
        public string AccessToken { get; set; } = null!;
        public DateTime ExpiresAtUtc { get; set; }
    }
}
